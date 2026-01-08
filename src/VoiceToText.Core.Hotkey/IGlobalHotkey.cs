namespace VoiceToText.Core.Hotkey;

/// <summary>
/// Represents a hotkey combination
/// </summary>
public record HotkeyDefinition(ModifierKeys Modifiers, VirtualKey Key);

/// <summary>
/// Modifier keys for hotkey combinations
/// </summary>
[Flags]
public enum ModifierKeys
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}

/// <summary>
/// Virtual key codes
/// </summary>
public enum VirtualKey
{
    Space = 0x20,
    A = 0x41,
    B = 0x42,
    C = 0x43,
    D = 0x44,
    E = 0x45,
    F = 0x46,
    G = 0x47,
    H = 0x48,
    I = 0x49,
    J = 0x4A,
    K = 0x4B,
    L = 0x4C,
    M = 0x4D,
    N = 0x4E,
    O = 0x4F,
    P = 0x50,
    Q = 0x51,
    R = 0x52,
    S = 0x53,
    T = 0x54,
    U = 0x55,
    V = 0x56,
    W = 0x57,
    X = 0x58,
    Y = 0x59,
    Z = 0x5A,
    F1 = 0x70,
    F2 = 0x71,
    F3 = 0x72,
    F4 = 0x73,
    F5 = 0x74,
    F6 = 0x75,
    F7 = 0x76,
    F8 = 0x77,
    F9 = 0x78,
    F10 = 0x79,
    F11 = 0x7A,
    F12 = 0x7B
}

/// <summary>
/// Interface for global hotkey management with hold-to-talk support
/// </summary>
public interface IGlobalHotkey : IDisposable
{
    /// <summary>
    /// Raised when the hotkey is pressed down
    /// </summary>
    event EventHandler? HotkeyPressed;

    /// <summary>
    /// Raised when the hotkey is released
    /// </summary>
    event EventHandler? HotkeyReleased;

    /// <summary>
    /// Gets or sets the current hotkey definition
    /// </summary>
    HotkeyDefinition Hotkey { get; set; }

    /// <summary>
    /// Gets whether the hotkey is currently being held down
    /// </summary>
    bool IsHotkeyDown { get; }

    /// <summary>
    /// Starts listening for the hotkey
    /// </summary>
    void Start();

    /// <summary>
    /// Stops listening for the hotkey
    /// </summary>
    void Stop();
}
