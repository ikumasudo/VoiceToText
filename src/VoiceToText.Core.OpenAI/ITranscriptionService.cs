namespace VoiceToText.Core.OpenAI;

/// <summary>
/// Options for transcription
/// </summary>
public class TranscriptionOptions
{
    /// <summary>
    /// Model to use for transcription.
    /// Default: gpt-4o-mini-transcribe
    /// Alternative: whisper-1
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini-transcribe";

    /// <summary>
    /// Language hint for transcription (ISO 639-1 code)
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Whether to add punctuation to the transcription
    /// </summary>
    public bool AddPunctuation { get; set; } = true;

    /// <summary>
    /// Whether to preserve line breaks in the transcription
    /// </summary>
    public bool PreserveNewlines { get; set; } = true;
}

/// <summary>
/// Result of a transcription request
/// </summary>
public class TranscriptionResult
{
    public bool Success { get; set; }
    public string? Text { get; set; }
    public string? Error { get; set; }
    public TranscriptionErrorType ErrorType { get; set; }
}

/// <summary>
/// Types of transcription errors
/// </summary>
public enum TranscriptionErrorType
{
    None,
    Authentication,
    RateLimit,
    InvalidRequest,
    Network,
    ServerError,
    Unknown
}

/// <summary>
/// Interface for transcription service
/// </summary>
public interface ITranscriptionService
{
    /// <summary>
    /// Gets or sets the API key
    /// </summary>
    string? ApiKey { get; set; }

    /// <summary>
    /// Transcribes audio data to text
    /// </summary>
    /// <param name="wavData">WAV audio data</param>
    /// <param name="options">Transcription options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transcription result</returns>
    Task<TranscriptionResult> TranscribeAsync(byte[] wavData, TranscriptionOptions? options = null, CancellationToken cancellationToken = default);
}
