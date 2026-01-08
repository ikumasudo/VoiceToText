using System.Windows;
using System.Windows.Media;
using VoiceToText.App.ViewModels;

namespace VoiceToText.App.Views;

public partial class StatusOverlay : Window
{
    private static readonly Brush IdleColor = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
    private static readonly Brush RecordingColor = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
    private static readonly Brush ProcessingColor = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Amber

    public StatusOverlay()
    {
        InitializeComponent();
        PositionWindow();
    }

    private void PositionWindow()
    {
        // Position in top-right corner
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 20;
        Top = workArea.Top + 20;
    }

    public void UpdateState(AppState state, string message)
    {
        StatusText.Text = message;
        StatusIndicator.Fill = state switch
        {
            AppState.Recording => RecordingColor,
            AppState.Processing => ProcessingColor,
            _ => IdleColor
        };

        // Show/hide based on state
        if (state == AppState.Idle)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void ShowTemporary(string message, int durationMs = 2000)
    {
        StatusText.Text = message;
        StatusIndicator.Fill = IdleColor;
        Show();

        _ = Task.Delay(durationMs).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                Hide();
            });
        });
    }
}
