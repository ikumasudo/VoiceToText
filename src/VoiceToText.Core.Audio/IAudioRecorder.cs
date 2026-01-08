namespace VoiceToText.Core.Audio;

/// <summary>
/// Represents the state of the audio recorder
/// </summary>
public enum RecordingState
{
    Stopped,
    Recording
}

/// <summary>
/// Event args for recording state changes
/// </summary>
public class RecordingStateChangedEventArgs : EventArgs
{
    public RecordingState State { get; }

    public RecordingStateChangedEventArgs(RecordingState state)
    {
        State = state;
    }
}

/// <summary>
/// Interface for audio recording
/// </summary>
public interface IAudioRecorder : IDisposable
{
    /// <summary>
    /// Gets the current recording state
    /// </summary>
    RecordingState State { get; }

    /// <summary>
    /// Gets or sets the maximum recording duration in seconds
    /// </summary>
    int MaxRecordingSeconds { get; set; }

    /// <summary>
    /// Raised when recording state changes
    /// </summary>
    event EventHandler<RecordingStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Raised when recording completes (either by stop or max duration)
    /// </summary>
    event EventHandler<RecordingCompletedEventArgs>? RecordingCompleted;

    /// <summary>
    /// Starts recording audio from the default microphone
    /// </summary>
    void StartRecording();

    /// <summary>
    /// Stops recording and returns the recorded audio as WAV data
    /// </summary>
    /// <returns>WAV file data as byte array</returns>
    byte[] StopRecording();
}

/// <summary>
/// Event args for recording completion
/// </summary>
public class RecordingCompletedEventArgs : EventArgs
{
    public byte[] WavData { get; }
    public TimeSpan Duration { get; }
    public bool WasMaxDurationReached { get; }

    public RecordingCompletedEventArgs(byte[] wavData, TimeSpan duration, bool wasMaxDurationReached)
    {
        WavData = wavData;
        Duration = duration;
        WasMaxDurationReached = wasMaxDurationReached;
    }
}
