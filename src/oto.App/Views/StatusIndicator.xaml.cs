using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using oto.App.ViewModels;

namespace oto.App.Views;

public partial class StatusIndicator : Window
{
    // Win32 API for hiding window from Alt+Tab
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    private Storyboard? _waveAnimation;
    private Storyboard? _spinnerAnimation;
    private bool _isDarkMode;

    // Theme colors
    private static readonly SolidColorBrush LightBackground = new(Color.FromArgb(0xE8, 0xFF, 0xFF, 0xFF));
    private static readonly SolidColorBrush LightBorder = new(Color.FromArgb(0x20, 0x00, 0x00, 0x00));
    private static readonly SolidColorBrush LightIdleIcon = new(Color.FromRgb(0x6B, 0x72, 0x80));

    private static readonly SolidColorBrush DarkBackground = new(Color.FromArgb(0xE8, 0x1F, 0x1F, 0x1F));
    private static readonly SolidColorBrush DarkBorder = new(Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF));
    private static readonly SolidColorBrush DarkIdleIcon = new(Color.FromRgb(0x9C, 0xA3, 0xAF));

    static StatusIndicator()
    {
        LightBackground.Freeze();
        LightBorder.Freeze();
        LightIdleIcon.Freeze();
        DarkBackground.Freeze();
        DarkBorder.Freeze();
        DarkIdleIcon.Freeze();
    }

    public StatusIndicator()
    {
        InitializeComponent();
        PositionWindow();
        _waveAnimation = (Storyboard)FindResource("WaveAnimation");
        _spinnerAnimation = (Storyboard)FindResource("SpinnerAnimation");

        // Detect and apply system theme
        DetectAndApplyTheme();

        // Listen for theme changes
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    private void PositionWindow()
    {
        // Position in bottom-right corner
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 10;
        Top = workArea.Bottom - Height - 10;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Set WS_EX_TOOLWINDOW to hide from Alt+Tab
        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
    }

    private void DetectAndApplyTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var appsUseLightTheme = (int?)key?.GetValue("AppsUseLightTheme") ?? 1;
            _isDarkMode = appsUseLightTheme == 0;
        }
        catch
        {
            _isDarkMode = false;
        }

        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (_isDarkMode)
        {
            MainBorder.Background = DarkBackground;
            MainBorder.BorderBrush = DarkBorder;
            IdleIcon.Fill = DarkIdleIcon;
        }
        else
        {
            MainBorder.Background = LightBackground;
            MainBorder.BorderBrush = LightBorder;
            IdleIcon.Fill = LightIdleIcon;
        }
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General)
        {
            Dispatcher.Invoke(() =>
            {
                DetectAndApplyTheme();
            });
        }
    }

    public void UpdateState(AppState state)
    {
        // Stop all animations
        _waveAnimation?.Stop();
        _spinnerAnimation?.Stop();

        // Hide all elements
        IdleIcon.Visibility = Visibility.Collapsed;
        RecordingIcon.Visibility = Visibility.Collapsed;
        ProcessingSpinner.Visibility = Visibility.Collapsed;
        DoneIcon.Visibility = Visibility.Collapsed;

        // Reset wave rings
        WaveRing1.Opacity = 0;
        WaveRing2.Opacity = 0;
        WaveRing3.Opacity = 0;

        // Show the appropriate element based on state
        switch (state)
        {
            case AppState.Idle:
                IdleIcon.Visibility = Visibility.Visible;
                break;

            case AppState.Recording:
                RecordingIcon.Visibility = Visibility.Visible;
                _waveAnimation?.Begin();
                break;

            case AppState.Processing:
                ProcessingSpinner.Visibility = Visibility.Visible;
                _spinnerAnimation?.Begin();
                break;

            case AppState.Done:
                DoneIcon.Visibility = Visibility.Visible;
                break;
        }
    }

    /// <summary>
    /// Updates the wave animation intensity based on audio level.
    /// Higher levels create more visible and faster waves.
    /// </summary>
    /// <param name="level">Normalized audio level (0.0 to 1.0)</param>
    public void UpdateAudioLevel(float level)
    {
        // 1. Scale the microphone icon (1.0x to 1.4x) - larger change
        var scale = 1.0 + (level * 0.4);
        RecordingIconTransform.ScaleX = scale;
        RecordingIconTransform.ScaleY = scale;

        // 2. Gradient color change (pink → bright red)
        // level 0.0: #F87171 (light pink-red)
        // level 1.0: #FF0000 (pure red)
        byte r = 0xFF;
        byte g = (byte)(0x71 - (level * 0x71));  // 113 → 0
        byte b = (byte)(0x71 - (level * 0x71));  // 113 → 0
        var color = Color.FromRgb(r, g, b);

        var brush = new SolidColorBrush(color);
        WaveRing1.Stroke = brush;
        WaveRing2.Stroke = brush;
        WaveRing3.Stroke = brush;
        RecordingIcon.Fill = brush;  // Icon color also changes

        // 3. Wave ring stroke thickness (2px to 6px) - thicker at high levels
        var thickness = 2 + (level * 4);
        WaveRing1.StrokeThickness = thickness;
        WaveRing2.StrokeThickness = thickness;
        WaveRing3.StrokeThickness = thickness;
    }

    protected override void OnClosed(EventArgs e)
    {
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        base.OnClosed(e);
    }
}
