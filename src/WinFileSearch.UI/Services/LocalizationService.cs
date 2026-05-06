using System.Globalization;
using System.Windows;
using Application = System.Windows.Application;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for managing application localization
/// </summary>
public interface ILocalizationService
{
    string CurrentLanguage { get; }
    CultureInfo CurrentCulture { get; }
    IReadOnlyList<string> AvailableLanguages { get; }
    void SetLanguage(string languageCode);
    string GetString(string key);
    event EventHandler? LanguageChanged;
}

public class LocalizationService : ILocalizationService
{
    private static LocalizationService? _instance;
    public static LocalizationService? Instance => _instance;

    private readonly Dictionary<string, string> _languageNames = new()
    {
        { "en", "English" },
        { "tr", "Türkçe" }
    };

    public string CurrentLanguage { get; private set; } = "en";
    public CultureInfo CurrentCulture { get; private set; } = new CultureInfo("en-US");
    public IReadOnlyList<string> AvailableLanguages => _languageNames.Keys.ToList().AsReadOnly();

    public event EventHandler? LanguageChanged;

    public LocalizationService(ISettingsService settingsService)
    {
        _instance = this;

        // Load saved language from settings
        var savedLanguage = settingsService.Settings.Language;
        if (string.IsNullOrEmpty(savedLanguage))
        {
            // Detect from system
            var culture = CultureInfo.CurrentUICulture;
            CurrentLanguage = culture.TwoLetterISOLanguageName == "tr" ? "tr" : "en";
        }
        else if (_languageNames.ContainsKey(savedLanguage))
        {
            CurrentLanguage = savedLanguage;
        }

        LoadLanguageResources(CurrentLanguage);
        SetCulture(CurrentLanguage);
    }

    public void SetLanguage(string languageCode)
    {
        if (!_languageNames.ContainsKey(languageCode))
            return;

        if (CurrentLanguage == languageCode)
            return;

        CurrentLanguage = languageCode;
        LoadLanguageResources(languageCode);
        SetCulture(languageCode);

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetCulture(string languageCode)
    {
        CurrentCulture = languageCode switch
        {
            "tr" => new CultureInfo("tr-TR"),
            _ => new CultureInfo("en-US")
        };
        CultureInfo.CurrentCulture = CurrentCulture;
        CultureInfo.CurrentUICulture = CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CurrentCulture;
        Thread.CurrentThread.CurrentUICulture = CurrentCulture;
    }

    public string GetString(string key)
    {
        try
        {
            var resource = Application.Current.TryFindResource(key);
            return resource as string ?? key;
        }
        catch
        {
            return key;
        }
    }

    public string GetLanguageName(string code)
    {
        return _languageNames.TryGetValue(code, out var name) ? name : code;
    }

    private static void LoadLanguageResources(string languageCode)
    {
        try
        {
            // Use pack URI for proper resource loading
            var resourcePath = $"pack://application:,,,/WinFileSearch.UI;component/Resources/Strings.{languageCode}.xaml";
            var uri = new Uri(resourcePath, UriKind.Absolute);
            var resourceDict = new ResourceDictionary { Source = uri };

            // Remove ALL existing language dictionaries (both en and tr)
            var toRemove = Application.Current.Resources.MergedDictionaries
                .Where(d => d.Source?.OriginalString.Contains("Strings.") == true)
                .ToList();

            foreach (var dict in toRemove)
            {
                Application.Current.Resources.MergedDictionaries.Remove(dict);
            }

            // Add new language dictionary
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
        catch (Exception ex)
        {
            // If loading fails, log and continue with existing resources
            System.Diagnostics.Debug.WriteLine($"Failed to load language resources: {ex.Message}");
        }
    }
}
