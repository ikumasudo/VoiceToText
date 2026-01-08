using System.Runtime.InteropServices;
using System.Windows;

namespace VoiceToText.Core.Paste;

/// <summary>
/// Text injector using clipboard + SendInput for Ctrl+V.
/// Optionally restores original clipboard content after injection.
/// </summary>
public sealed class ClipboardTextInjector : ITextInjector
{
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_V = 0x56;

    // Properly aligned INPUT structure for 64-bit Windows
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    public TextInjectionOptions Options { get; set; } = new();

    public async Task<TextInjectionResult> InjectTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new TextInjectionResult
            {
                Success = false,
                Error = "No text to inject"
            };
        }

        // Execute clipboard operations on UI thread
        return await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            string? originalClipboardContent = null;
            bool hadTextContent = false;

            try
            {
                // Save original clipboard content if restore is enabled
                if (Options.RestoreClipboard)
                {
                    try
                    {
                        if (Clipboard.ContainsText())
                        {
                            originalClipboardContent = Clipboard.GetText();
                            hadTextContent = true;
                        }
                    }
                    catch
                    {
                        // Ignore clipboard read errors
                    }
                }

                // Set text to clipboard with retry
                SetClipboardText(text);

                // Small delay to ensure clipboard is ready
                Thread.Sleep(Options.PasteDelayMs);

                // Send Ctrl+V
                SendCtrlV();

                // Small delay before restoring clipboard
                if (Options.RestoreClipboard)
                {
                    Thread.Sleep(150);

                    try
                    {
                        if (hadTextContent && originalClipboardContent != null)
                        {
                            SetClipboardText(originalClipboardContent);
                        }
                        else if (!hadTextContent)
                        {
                            Clipboard.Clear();
                        }
                    }
                    catch
                    {
                        // Ignore clipboard restore errors
                    }
                }

                return new TextInjectionResult { Success = true };
            }
            catch (Exception ex)
            {
                return new TextInjectionResult
                {
                    Success = false,
                    Error = $"Failed to inject text: {ex.Message}"
                };
            }
        });
    }

    private static void SetClipboardText(string text)
    {
        // Retry clipboard operation with exponential backoff
        Exception? lastException = null;
        for (int i = 0; i < 10; i++)
        {
            try
            {
                Clipboard.SetDataObject(text, true);
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Thread.Sleep(20 * (i + 1));
            }
        }
        throw lastException ?? new InvalidOperationException("Failed to set clipboard text");
    }

    private static void SendCtrlV()
    {
        var inputs = new INPUT[4];

        // Ctrl down
        inputs[0] = CreateKeyInput(VK_CONTROL, false);
        // V down
        inputs[1] = CreateKeyInput(VK_V, false);
        // V up
        inputs[2] = CreateKeyInput(VK_V, true);
        // Ctrl up
        inputs[3] = CreateKeyInput(VK_CONTROL, true);

        var size = Marshal.SizeOf<INPUT>();
        var result = SendInput(4, inputs, size);
        if (result != 4)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"SendInput failed. Sent {result}/4 inputs. Error code: {error}");
        }
    }

    private static INPUT CreateKeyInput(ushort virtualKey, bool keyUp)
    {
        return new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    wScan = 0,
                    dwFlags = keyUp ? KEYEVENTF_KEYUP : 0u,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
    }
}
