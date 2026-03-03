using System.IO;
using System.Text.Json;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Application settings model
/// </summary>
public class AppSettings
{
    public bool MinimizeToTray { get; set; } = true;
    public bool StartMinimized { get; set; } = false;
    public bool StartWithWindows { get; set; } = false;
    public bool BackgroundIndexing { get; set; } = true;
    public bool GlobalHotkeyEnabled { get; set; } = true;
    public int SearchDebounceMs { get; set; } = 300;
    public int MaxSearchResults { get; set; } = 100;
    public string Theme { get; set; } = "Dark";
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

/// <summary>
/// Service for managing application settings
/// </summary>
public interface ISettingsService
{
    AppSettings Settings { get; }
    void Save();
    void Load();
    event EventHandler? SettingsChanged;
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppSettings Settings { get; private set; } = new();
    public event EventHandler? SettingsChanged;

    public SettingsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinFileSearch");

        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _settingsFilePath = Path.Combine(appDataPath, "settings.json");
        Load();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (settings != null)
                {
                    Settings = settings;
                }
            }
        }
        catch
        {
            // If loading fails, use default settings
            Settings = new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            Settings.LastUpdated = DateTime.Now;
            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // Silently fail if saving doesn't work
        }
    }
}
