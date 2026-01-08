using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceToText.App.Models;
using VoiceToText.Core.Audio;
using VoiceToText.Core.Hotkey;
using VoiceToText.Core.OpenAI;
using VoiceToText.Core.Paste;

namespace VoiceToText.App.ViewModels;

/// <summary>
/// Application state
/// </summary>
public enum AppState
{
    Idle,
    Recording,
    Processing
}

/// <summary>
/// Main view model that orchestrates all services
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IGlobalHotkey _hotkey;
    private readonly IAudioRecorder _audioRecorder;
    private readonly ITranscriptionService _transcriptionService;
    private readonly ITextInjector _textInjector;
    private readonly TranscriptionHistory _history;

    private AppSettings _settings;
    private bool _isDisposed;

    [ObservableProperty]
    private AppState _state = AppState.Idle;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isApiKeyConfigured;

    public TranscriptionHistory History => _history;
    public AppSettings Settings => _settings;

    public event EventHandler<string>? NotificationRequested;
    public event EventHandler<string>? ErrorOccurred;

    public MainViewModel()
    {
        _hotkey = new GlobalHotkeyManager();
        _audioRecorder = new AudioRecorder();
        _transcriptionService = new OpenAITranscriptionService();
        _textInjector = new ClipboardTextInjector();
        _history = new TranscriptionHistory();
        _settings = SettingsManager.Load();

        ApplySettings();

        _hotkey.HotkeyPressed += OnHotkeyPressed;
        _hotkey.HotkeyReleased += OnHotkeyReleased;
        _audioRecorder.RecordingCompleted += OnRecordingCompleted;
    }

    public void Start()
    {
        try
        {
            _hotkey.Start();
            StatusMessage = IsApiKeyConfigured ? "Ready (Ctrl+Alt+Space to record)" : "API Key not configured";
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to start hotkey listener: {ex.Message}");
        }
    }

    public void Stop()
    {
        _hotkey.Stop();
        if (_audioRecorder.State == RecordingState.Recording)
        {
            _audioRecorder.StopRecording();
        }
    }

    public void ApplySettings()
    {
        _hotkey.Hotkey = new HotkeyDefinition(_settings.Hotkey.Modifiers, _settings.Hotkey.Key);
        _audioRecorder.MaxRecordingSeconds = _settings.MaxRecordingSeconds;
        _transcriptionService.ApiKey = _settings.ApiKey;
        _textInjector.Options = new TextInjectionOptions { RestoreClipboard = _settings.RestoreClipboard };

        IsApiKeyConfigured = !string.IsNullOrWhiteSpace(_settings.ApiKey);
    }

    public void UpdateSettings(AppSettings settings)
    {
        _settings = settings;
        SettingsManager.Save(_settings);
        ApplySettings();
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        if (State != AppState.Idle) return;
        if (!IsApiKeyConfigured)
        {
            ErrorOccurred?.Invoke(this, "API Key is not configured. Please set it in Settings.");
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            State = AppState.Recording;
            StatusMessage = "Recording...";
            _audioRecorder.StartRecording();
        });
    }

    private void OnHotkeyReleased(object? sender, EventArgs e)
    {
        if (State != AppState.Recording) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            State = AppState.Processing;
            StatusMessage = "Processing...";
            _audioRecorder.StopRecording();
        });
    }

    private async void OnRecordingCompleted(object? sender, RecordingCompletedEventArgs e)
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            if (e.WavData.Length == 0)
            {
                State = AppState.Idle;
                StatusMessage = "No audio recorded";
                return;
            }

            try
            {
                var options = new TranscriptionOptions
                {
                    Model = _settings.Model,
                    Language = _settings.Language,
                    AddPunctuation = _settings.AddPunctuation,
                    PreserveNewlines = _settings.PreserveNewlines
                };

                var result = await _transcriptionService.TranscribeAsync(e.WavData, options);

                if (result.Success && !string.IsNullOrEmpty(result.Text))
                {
                    var injectResult = await _textInjector.InjectTextAsync(result.Text);

                    if (injectResult.Success)
                    {
                        _history.Add(result.Text, e.Duration, true);
                        StatusMessage = "Done!";
                        NotificationRequested?.Invoke(this, "Text pasted successfully");
                    }
                    else
                    {
                        _history.Add(result.Text, e.Duration, false, injectResult.Error);
                        StatusMessage = "Paste failed";
                        ErrorOccurred?.Invoke(this, $"Failed to paste: {injectResult.Error}");
                    }
                }
                else
                {
                    var errorMsg = GetUserFriendlyError(result);
                    _history.Add("", e.Duration, false, errorMsg);
                    StatusMessage = "Transcription failed";
                    ErrorOccurred?.Invoke(this, errorMsg);
                }
            }
            catch (Exception ex)
            {
                _history.Add("", e.Duration, false, ex.Message);
                StatusMessage = "Error";
                ErrorOccurred?.Invoke(this, $"Error: {ex.Message}");
            }
            finally
            {
                State = AppState.Idle;

                // Reset status after a delay
                _ = Task.Delay(2000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (State == AppState.Idle)
                        {
                            StatusMessage = "Ready";
                        }
                    });
                });
            }
        });
    }

    private static string GetUserFriendlyError(TranscriptionResult result)
    {
        return result.ErrorType switch
        {
            TranscriptionErrorType.Authentication => "Invalid API key. Please check your settings.",
            TranscriptionErrorType.RateLimit => "Rate limit exceeded. Please wait and try again.",
            TranscriptionErrorType.InvalidRequest => $"Invalid request: {result.Error}",
            TranscriptionErrorType.Network => "Network error. Please check your connection.",
            TranscriptionErrorType.ServerError => "OpenAI server error. Please try again later.",
            _ => result.Error ?? "Unknown error occurred"
        };
    }

    [RelayCommand]
    private void CopyHistoryText(HistoryEntry? entry)
    {
        if (entry != null && !string.IsNullOrEmpty(entry.Text))
        {
            try
            {
                Clipboard.SetText(entry.Text);
                NotificationRequested?.Invoke(this, "Copied to clipboard");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to copy: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        Stop();
        _hotkey.Dispose();
        _audioRecorder.Dispose();
        if (_transcriptionService is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _isDisposed = true;
    }
}
