using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using VoiceToText.App.ViewModels;
using VoiceToText.App.Views;

namespace VoiceToText.App;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private TrayToolTip? _trayToolTip;
    private MainViewModel? _viewModel;
    private StatusIndicator? _statusIndicator;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single instance check
        var mutex = new System.Threading.Mutex(true, "VoiceToText_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("VoiceToText is already running.", "VoiceToText", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // Initialize view model
        _viewModel = new MainViewModel();
        _viewModel.NotificationRequested += OnNotificationRequested;
        _viewModel.ErrorOccurred += OnErrorOccurred;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Initialize status indicator (always visible in bottom-right corner)
        _statusIndicator = new StatusIndicator();
        _statusIndicator.Show();

        // Create tray tooltip
        _trayToolTip = new TrayToolTip();
        _trayToolTip.UpdateHotkey(_viewModel.Settings.Hotkey.ToDisplayString());

        // Create tray icon
        _trayIcon = new TaskbarIcon
        {
            Icon = CreateDefaultIcon(),
            TrayToolTip = _trayToolTip
        };

        // Create context menu
        var contextMenu = new System.Windows.Controls.ContextMenu();

        var settingsMenuItem = new System.Windows.Controls.MenuItem { Header = "Settings" };
        settingsMenuItem.Click += OnSettingsClick;
        contextMenu.Items.Add(settingsMenuItem);

        var historyMenuItem = new System.Windows.Controls.MenuItem { Header = "History" };
        historyMenuItem.Click += OnHistoryClick;
        contextMenu.Items.Add(historyMenuItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var exitMenuItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exitMenuItem.Click += OnExitClick;
        contextMenu.Items.Add(exitMenuItem);

        _trayIcon.ContextMenu = contextMenu;

        // Start listening for hotkeys
        _viewModel.Start();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_viewModel == null || _statusIndicator == null) return;

        Dispatcher.Invoke(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.State):
                    _statusIndicator.UpdateState(_viewModel.State);
                    break;

                case nameof(MainViewModel.AudioLevel):
                    if (_viewModel.State == AppState.Recording)
                    {
                        _statusIndicator.UpdateAudioLevel(_viewModel.AudioLevel);
                    }
                    break;
            }
        });
    }

    private void OnNotificationRequested(object? sender, string message)
    {
    }

    private void OnErrorOccurred(object? sender, string error)
    {
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        var settingsWindow = new SettingsWindow(_viewModel.Settings);
        if (settingsWindow.ShowDialog() == true && settingsWindow.Result != null)
        {
            _viewModel.Stop();
            _viewModel.UpdateSettings(settingsWindow.Result);
            _viewModel.Start();

            // Update tray tooltip with new hotkey
            _trayToolTip?.UpdateHotkey(_viewModel.Settings.Hotkey.ToDisplayString());
        }
    }

    private void OnHistoryClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        var historyWindow = new HistoryWindow(_viewModel.History);
        historyWindow.Show();
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _viewModel?.Stop();
        _viewModel?.Dispose();
        _trayIcon?.Dispose();
        _statusIndicator?.Close();
        base.OnExit(e);
    }

    private static Icon CreateDefaultIcon()
    {
        // Create a modern flat-style icon with purple gradient
        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);

        // Enable high-quality rendering
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(Color.Transparent);

        // Define colors (Material Design Purple)
        var purpleLight = Color.FromArgb(171, 71, 188);   // #AB47BC
        var purpleDark = Color.FromArgb(123, 31, 162);    // #7B1FA2
        var highlight = Color.FromArgb(100, 225, 190, 231); // Semi-transparent #E1BEE7
        var shadow = Color.FromArgb(40, 0, 0, 0);         // Light shadow

        // Draw shadow for microphone head
        using (var shadowBrush = new SolidBrush(shadow))
        {
            graphics.FillEllipse(shadowBrush, 8, 5, 16, 18);
        }

        // Microphone head with gradient
        using (var gradientBrush = new LinearGradientBrush(
            new Rectangle(7, 3, 18, 18),
            purpleLight,
            purpleDark,
            LinearGradientMode.Vertical))
        {
            graphics.FillEllipse(gradientBrush, 7, 3, 18, 18);
        }

        // Microphone stand
        using (var standBrush = new LinearGradientBrush(
            new Rectangle(13, 20, 6, 6),
            purpleLight,
            purpleDark,
            LinearGradientMode.Vertical))
        {
            graphics.FillRectangle(standBrush, 13, 20, 6, 6);
        }

        // Microphone base (arc shape)
        using (var baseBrush = new SolidBrush(purpleDark))
        using (var pen = new Pen(purpleDark, 2.5f))
        {
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            graphics.DrawArc(pen, 9, 24, 14, 6, 0, 180);
        }

        // Highlight on microphone head (glossy effect)
        using (var highlightBrush = new SolidBrush(highlight))
        {
            graphics.FillEllipse(highlightBrush, 10, 6, 6, 5);
        }

        return Icon.FromHandle(bitmap.GetHicon());
    }
}
