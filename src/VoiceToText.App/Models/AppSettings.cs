using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VoiceToText.Core.Hotkey;

namespace VoiceToText.App.Models;

/// <summary>
/// Hotkey configuration
/// </summary>
public class HotkeyConfig
{
    public ModifierKeys Modifiers { get; set; } = ModifierKeys.Control | ModifierKeys.Alt;
    public VirtualKey Key { get; set; } = VirtualKey.Space;
}

/// <summary>
/// Application settings
/// </summary>
public class AppSettings
{
    public HotkeyConfig Hotkey { get; set; } = new();
    public string Model { get; set; } = "gpt-4o-mini-transcribe";
    public int MaxRecordingSeconds { get; set; } = 60;
    public bool AddPunctuation { get; set; } = true;
    public bool PreserveNewlines { get; set; } = true;
    public bool RestoreClipboard { get; set; } = true;
    public string? Language { get; set; }

    [JsonIgnore]
    public string? ApiKey { get; set; }
}

/// <summary>
/// Manages application settings with DPAPI encryption for API key
/// </summary>
public static class SettingsManager
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "VoiceToText");

    private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");
    private static readonly string ApiKeyFile = Path.Combine(SettingsFolder, "apikey.dat");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static AppSettings Load()
    {
        var settings = new AppSettings();

        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }

            // Load encrypted API key
            if (File.Exists(ApiKeyFile))
            {
                var encryptedData = File.ReadAllBytes(ApiKeyFile);
                var decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
                settings.ApiKey = Encoding.UTF8.GetString(decryptedData);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }

        return settings;
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsFolder);

            // Save settings (without API key)
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFile, json);

            // Save encrypted API key
            if (!string.IsNullOrEmpty(settings.ApiKey))
            {
                var data = Encoding.UTF8.GetBytes(settings.ApiKey);
                var encryptedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(ApiKeyFile, encryptedData);
            }
            else if (File.Exists(ApiKeyFile))
            {
                File.Delete(ApiKeyFile);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}
