using System.Drawing;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using VoiceToText.App.ViewModels;
using VoiceToText.App.Views;

namespace VoiceToText.App;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
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

        // Create tray icon
        _trayIcon = new TaskbarIcon
        {
            Icon = CreateDefaultIcon(),
            ToolTipText = "VoiceToText - Ctrl+Alt+Space to record"
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
        // Create a simple icon programmatically
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);

        // Draw a microphone-like icon
        graphics.Clear(Color.Transparent);
        using var brush = new SolidBrush(Color.FromArgb(33, 150, 243)); // Blue
        graphics.FillEllipse(brush, 4, 2, 8, 8);
        graphics.FillRectangle(brush, 6, 10, 4, 4);

        return Icon.FromHandle(bitmap.GetHicon());
    }
}
