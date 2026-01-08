using System.Collections.ObjectModel;

namespace VoiceToText.App.Models;

/// <summary>
/// A single transcription history entry
/// </summary>
public class HistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string Text { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Manages transcription history
/// </summary>
public class TranscriptionHistory
{
    private const int MaxEntries = 20;
    private readonly ObservableCollection<HistoryEntry> _entries = [];

    public ObservableCollection<HistoryEntry> Entries => _entries;

    public void Add(string text, TimeSpan duration, bool isSuccess, string? error = null)
    {
        var entry = new HistoryEntry
        {
            Timestamp = DateTime.Now,
            Text = text,
            Duration = duration,
            IsSuccess = isSuccess,
            Error = error
        };

        _entries.Insert(0, entry);

        // Keep only the last MaxEntries
        while (_entries.Count > MaxEntries)
        {
            _entries.RemoveAt(_entries.Count - 1);
        }
    }

    public void Clear()
    {
        _entries.Clear();
    }
}
