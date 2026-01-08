namespace VoiceToText.Core.Paste;

/// <summary>
/// Options for text injection
/// </summary>
public class TextInjectionOptions
{
    /// <summary>
    /// Whether to restore the original clipboard content after injection
    /// </summary>
    public bool RestoreClipboard { get; set; } = true;

    /// <summary>
    /// Delay in milliseconds before sending Ctrl+V after setting clipboard
    /// </summary>
    public int PasteDelayMs { get; set; } = 50;
}

/// <summary>
/// Result of a text injection operation
/// </summary>
public class TextInjectionResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Interface for injecting text into the focused application
/// </summary>
public interface ITextInjector
{
    /// <summary>
    /// Gets or sets the injection options
    /// </summary>
    TextInjectionOptions Options { get; set; }

    /// <summary>
    /// Injects text into the currently focused application using clipboard + Ctrl+V
    /// </summary>
    /// <param name="text">Text to inject</param>
    /// <returns>Injection result</returns>
    Task<TextInjectionResult> InjectTextAsync(string text);
}
