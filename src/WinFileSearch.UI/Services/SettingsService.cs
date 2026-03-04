using System.IO;
using System.Text.Json;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Application settings model containing all configurable options.
/// Settings are persisted to JSON file in the user's local app data folder.
/// </summary>
public class AppSettings
{
    /// <summary>Gets or sets whether to minimize to system tray instead of taskbar.</summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>Gets or sets whether to start the application minimized.</summary>
    public bool StartMinimized { get; set; } = false;

    /// <summary>Gets or sets whether to start the application with Windows.</summary>
    public bool StartWithWindows { get; set; } = false;

    /// <summary>Gets or sets whether to enable background file indexing.</summary>
    public bool BackgroundIndexing { get; set; } = true;

    /// <summary>Gets or sets whether the global hotkey (Win+Shift+F) is enabled.</summary>
    public bool GlobalHotkeyEnabled { get; set; } = true;

    /// <summary>Gets or sets the search debounce delay in milliseconds.</summary>
    public int SearchDebounceMs { get; set; } = 300;

    /// <summary>Gets or sets the maximum number of search results to display.</summary>
    public int MaxSearchResults { get; set; } = 100;

    /// <summary>Gets or sets the application theme name.</summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>Gets or sets the timestamp of the last settings update.</summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

/// <summary>
/// Service for managing application settings persistence.
/// Provides load, save, and change notification functionality.
/// </summary>
public interface ISettingsService
{
    /// <summary>Gets the current application settings.</summary>
    AppSettings Settings { get; }

    /// <summary>Saves the current settings to the settings file.</summary>
    void Save();

    /// <summary>Loads settings from the settings file.</summary>
    void Load();

    /// <summary>Occurs when settings are saved.</summary>
    event EventHandler? SettingsChanged;
}

/// <summary>
/// Default implementation of <see cref="ISettingsService"/>.
/// Persists settings to JSON file in %LocalAppData%\WinFileSearch\settings.json.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc />
    public AppSettings Settings { get; private set; } = new();

    /// <inheritdoc />
    public event EventHandler? SettingsChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class
    /// and loads existing settings from disk.
    /// </summary>
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

    /// <inheritdoc />
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
