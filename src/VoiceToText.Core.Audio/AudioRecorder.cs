using NAudio.Wave;

namespace VoiceToText.Core.Audio;

/// <summary>
/// Audio recorder using NAudio for microphone capture.
/// Records PCM 16-bit mono at 16kHz and outputs WAV format.
/// </summary>
public sealed class AudioRecorder : IAudioRecorder
{
    private const int SampleRate = 16000;
    private const int BitsPerSample = 16;
    private const int Channels = 1;

    private WaveInEvent? _waveIn;
    private MemoryStream? _audioStream;
    private WaveFileWriter? _waveWriter;
    private System.Timers.Timer? _maxDurationTimer;
    private DateTime _recordingStartTime;
    private bool _isDisposed;
    private readonly object _lock = new();

    public RecordingState State { get; private set; } = RecordingState.Stopped;
    public int MaxRecordingSeconds { get; set; } = 60;

    public event EventHandler<RecordingStateChangedEventArgs>? StateChanged;
    public event EventHandler<RecordingCompletedEventArgs>? RecordingCompleted;

    public void StartRecording()
    {
        lock (_lock)
        {
            if (State == RecordingState.Recording)
            {
                return;
            }

            _audioStream = new MemoryStream();
            _waveWriter = new WaveFileWriter(_audioStream, new WaveFormat(SampleRate, BitsPerSample, Channels));

            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels),
                BufferMilliseconds = 50
            };

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            _recordingStartTime = DateTime.UtcNow;
            _waveIn.StartRecording();

            // Set up max duration timer
            _maxDurationTimer = new System.Timers.Timer(MaxRecordingSeconds * 1000);
            _maxDurationTimer.Elapsed += OnMaxDurationReached;
            _maxDurationTimer.AutoReset = false;
            _maxDurationTimer.Start();

            State = RecordingState.Recording;
            StateChanged?.Invoke(this, new RecordingStateChangedEventArgs(State));
        }
    }

    public byte[] StopRecording()
    {
        lock (_lock)
        {
            return StopRecordingInternal(false);
        }
    }

    private byte[] StopRecordingInternal(bool wasMaxDurationReached)
    {
        if (State != RecordingState.Recording)
        {
            return [];
        }

        _maxDurationTimer?.Stop();
        _maxDurationTimer?.Dispose();
        _maxDurationTimer = null;

        _waveIn?.StopRecording();

        var duration = DateTime.UtcNow - _recordingStartTime;

        // Finalize the WAV file
        _waveWriter?.Flush();
        _waveWriter?.Dispose();
        _waveWriter = null;

        var wavData = _audioStream?.ToArray() ?? [];

        _audioStream?.Dispose();
        _audioStream = null;

        _waveIn?.Dispose();
        _waveIn = null;

        State = RecordingState.Stopped;
        StateChanged?.Invoke(this, new RecordingStateChangedEventArgs(State));
        RecordingCompleted?.Invoke(this, new RecordingCompletedEventArgs(wavData, duration, wasMaxDurationReached));

        return wavData;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        lock (_lock)
        {
            _waveWriter?.Write(e.Buffer, 0, e.BytesRecorded);
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            System.Diagnostics.Debug.WriteLine($"Recording error: {e.Exception.Message}");
        }
    }

    private void OnMaxDurationReached(object? sender, System.Timers.ElapsedEventArgs e)
    {
        lock (_lock)
        {
            if (State == RecordingState.Recording)
            {
                StopRecordingInternal(true);
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        lock (_lock)
        {
            _maxDurationTimer?.Dispose();
            _waveWriter?.Dispose();
            _audioStream?.Dispose();
            _waveIn?.Dispose();
        }

        _isDisposed = true;
    }
}
