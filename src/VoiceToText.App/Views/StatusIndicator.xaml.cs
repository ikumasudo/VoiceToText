using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VoiceToText.App.ViewModels;

namespace VoiceToText.App.Views;

public partial class StatusIndicator : Window
{
    private Storyboard? _pulseAnimation;
    private const double MaxLevelHeight = 32.0;

    // Cached brushes for level colors
    private static readonly SolidColorBrush GreenBrush = new(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly SolidColorBrush OrangeBrush = new(Color.FromRgb(0xFF, 0x98, 0x00));
    private static readonly SolidColorBrush RedBrush = new(Color.FromRgb(0xFF, 0x57, 0x22));

    static StatusIndicator()
    {
        GreenBrush.Freeze();
        OrangeBrush.Freeze();
        RedBrush.Freeze();
    }

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
        RecordingPanel.Visibility = Visibility.Collapsed;
        ProcessingIcon.Visibility = Visibility.Collapsed;
        DoneIcon.Visibility = Visibility.Collapsed;

        // Show the appropriate icon based on state
        switch (state)
        {
            case AppState.Idle:
                IdleIcon.Visibility = Visibility.Visible;
                break;

            case AppState.Recording:
                RecordingPanel.Visibility = Visibility.Visible;
                LevelBar.Height = 0; // Reset level bar
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

    /// <summary>
    /// Updates the audio level meter
    /// </summary>
    /// <param name="level">Normalized audio level (0.0 to 1.0)</param>
    public void UpdateAudioLevel(float level)
    {
        // Update bar height based on level
        LevelBar.Height = level * MaxLevelHeight;

        // Update color based on level: green -> orange -> red
        LevelBar.Background = level switch
        {
            > 0.8f => RedBrush,
            > 0.6f => OrangeBrush,
            _ => GreenBrush
        };
    }
}
