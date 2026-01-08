using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VoiceToText.Core.Hotkey;

/// <summary>
/// Global hotkey manager using WH_KEYBOARD_LL low-level keyboard hook.
/// Supports hold-to-talk by detecting both key press and release.
/// </summary>
public sealed class GlobalHotkeyManager : IGlobalHotkey
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private IntPtr _hookId = IntPtr.Zero;
    private LowLevelKeyboardProc? _proc;
    private bool _isHotkeyDown;
    private bool _isDisposed;

    public event EventHandler? HotkeyPressed;
    public event EventHandler? HotkeyReleased;

    public HotkeyDefinition Hotkey { get; set; } = new(ModifierKeys.Control | ModifierKeys.Alt, VirtualKey.Space);

    public bool IsHotkeyDown => _isHotkeyDown;

    public void Start()
    {
        if (_hookId != IntPtr.Zero) return;

        _proc = HookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName!), 0);

        if (_hookId == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to set keyboard hook. Error: {Marshal.GetLastWin32Error()}");
        }
    }

    public void Stop()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        if (_isHotkeyDown)
        {
            _isHotkeyDown = false;
            HotkeyReleased?.Invoke(this, EventArgs.Empty);
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var vkCode = (int)hookStruct.vkCode;
            var targetKey = (int)Hotkey.Key;

            if (vkCode == targetKey)
            {
                bool isKeyDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
                bool isKeyUp = wParam == WM_KEYUP || wParam == WM_SYSKEYUP;

                if (isKeyDown && !_isHotkeyDown && AreModifiersPressed())
                {
                    _isHotkeyDown = true;
                    HotkeyPressed?.Invoke(this, EventArgs.Empty);
                }
                else if (isKeyUp && _isHotkeyDown)
                {
                    _isHotkeyDown = false;
                    HotkeyReleased?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private bool AreModifiersPressed()
    {
        var required = Hotkey.Modifiers;

        bool ctrlRequired = (required & ModifierKeys.Control) != 0;
        bool altRequired = (required & ModifierKeys.Alt) != 0;
        bool shiftRequired = (required & ModifierKeys.Shift) != 0;
        bool winRequired = (required & ModifierKeys.Win) != 0;

        bool ctrlPressed = IsKeyPressed(0x11) || IsKeyPressed(0xA2) || IsKeyPressed(0xA3); // VK_CONTROL, VK_LCONTROL, VK_RCONTROL
        bool altPressed = IsKeyPressed(0x12) || IsKeyPressed(0xA4) || IsKeyPressed(0xA5); // VK_MENU, VK_LMENU, VK_RMENU
        bool shiftPressed = IsKeyPressed(0x10) || IsKeyPressed(0xA0) || IsKeyPressed(0xA1); // VK_SHIFT, VK_LSHIFT, VK_RSHIFT
        bool winPressed = IsKeyPressed(0x5B) || IsKeyPressed(0x5C); // VK_LWIN, VK_RWIN

        return ctrlRequired == ctrlPressed &&
               altRequired == altPressed &&
               shiftRequired == shiftPressed &&
               winRequired == winPressed;
    }

    private static bool IsKeyPressed(int vkCode)
    {
        return (GetAsyncKeyState(vkCode) & 0x8000) != 0;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        Stop();
        _isDisposed = true;
    }
}
