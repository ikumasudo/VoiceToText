using System.Windows;
using System.Windows.Controls;
using VoiceToText.App.Models;
using VoiceToText.Core.Hotkey;

namespace VoiceToText.App.Views;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public AppSettings? Result { get; private set; }

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        LoadSettings();
    }

    private void LoadSettings()
    {
        // API Key
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            ApiKeyBox.Password = _settings.ApiKey;
        }

        // Model
        foreach (ComboBoxItem item in ModelComboBox.Items)
        {
            if (item.Content.ToString() == _settings.Model)
            {
                ModelComboBox.SelectedItem = item;
                break;
            }
        }

        // Modifiers
        var modifiersTag = ((int)_settings.Hotkey.Modifiers).ToString();
        foreach (ComboBoxItem item in ModifiersComboBox.Items)
        {
            if (item.Tag.ToString() == modifiersTag)
            {
                ModifiersComboBox.SelectedItem = item;
                break;
            }
        }
        if (ModifiersComboBox.SelectedItem == null)
        {
            ModifiersComboBox.SelectedIndex = 0;
        }

        // Key
        var keyTag = ((int)_settings.Hotkey.Key).ToString();
        foreach (ComboBoxItem item in KeyComboBox.Items)
        {
            if (item.Tag.ToString() == keyTag)
            {
                KeyComboBox.SelectedItem = item;
                break;
            }
        }
        if (KeyComboBox.SelectedItem == null)
        {
            KeyComboBox.SelectedIndex = 0;
        }

        // Max Duration
        foreach (ComboBoxItem item in MaxDurationComboBox.Items)
        {
            if (item.Content.ToString() == _settings.MaxRecordingSeconds.ToString())
            {
                MaxDurationComboBox.SelectedItem = item;
                break;
            }
        }

        // Language
        LanguageBox.Text = _settings.Language ?? "";

        // Options
        AddPunctuationCheckBox.IsChecked = _settings.AddPunctuation;
        PreserveNewlinesCheckBox.IsChecked = _settings.PreserveNewlines;
        RestoreClipboardCheckBox.IsChecked = _settings.RestoreClipboard;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        Result = new AppSettings
        {
            ApiKey = ApiKeyBox.Password,
            Model = (ModelComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "gpt-4o-mini-transcribe",
            Hotkey = new HotkeyConfig
            {
                Modifiers = (ModifierKeys)int.Parse((ModifiersComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "6"),
                Key = (VirtualKey)int.Parse((KeyComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "32")
            },
            MaxRecordingSeconds = int.Parse((MaxDurationComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "60"),
            Language = string.IsNullOrWhiteSpace(LanguageBox.Text) ? null : LanguageBox.Text.Trim(),
            AddPunctuation = AddPunctuationCheckBox.IsChecked ?? true,
            PreserveNewlines = PreserveNewlinesCheckBox.IsChecked ?? true,
            RestoreClipboard = RestoreClipboardCheckBox.IsChecked ?? true
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
