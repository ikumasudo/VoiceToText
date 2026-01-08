using System.Windows;
using System.Windows.Controls;
using VoiceToText.App.Models;

namespace VoiceToText.App.Views;

public partial class HistoryWindow : Window
{
    private readonly TranscriptionHistory _history;

    public HistoryWindow(TranscriptionHistory history)
    {
        InitializeComponent();
        _history = history;
        HistoryList.ItemsSource = _history.Entries;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is HistoryEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.Text))
            {
                try
                {
                    Clipboard.SetText(entry.Text);
                    MessageBox.Show("Copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to copy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Clear all history?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _history.Clear();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
