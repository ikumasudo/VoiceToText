using System.Windows;
using System.Windows.Media.Animation;
using VoiceToText.App.ViewModels;

namespace VoiceToText.App.Views;

public partial class StatusIndicator : Window
{
    private Storyboard? _pulseAnimation;

    public StatusIndicator()
    {
        InitializeComponent();
        PositionWindow();
        _pulseAnimation = (Storyboard)FindResource("PulseAnimation");
    }

    private void PositionWindow()
    {
        // Position in bottom-right corner
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 10;
        Top = workArea.Bottom - Height - 10;
    }

    public void UpdateState(AppState state)
    {
        // Stop any running animation
        _pulseAnimation?.Stop();

        // Hide all icons
        IdleIcon.Visibility = Visibility.Collapsed;
        RecordingIcon.Visibility = Visibility.Collapsed;
        ProcessingIcon.Visibility = Visibility.Collapsed;
        DoneIcon.Visibility = Visibility.Collapsed;

        // Show the appropriate icon based on state
        switch (state)
        {
            case AppState.Idle:
                IdleIcon.Visibility = Visibility.Visible;
                break;

            case AppState.Recording:
                RecordingIcon.Visibility = Visibility.Visible;
                break;

            case AppState.Processing:
                ProcessingIcon.Visibility = Visibility.Visible;
                _pulseAnimation?.Begin();
                break;

            case AppState.Done:
                DoneIcon.Visibility = Visibility.Visible;
                break;
        }
    }
}
