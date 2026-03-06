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
    IReadOnlyList<string> AvailableLanguages { get; }
    void SetLanguage(string languageCode);
    string GetString(string key);
    event EventHandler? LanguageChanged;
}

public class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, string> _languageNames = new()
    {
        { "en", "English" },
        { "tr", "Türkçe" }
    };

    public string CurrentLanguage { get; private set; } = "en";
    public IReadOnlyList<string> AvailableLanguages => _languageNames.Keys.ToList().AsReadOnly();

    public event EventHandler? LanguageChanged;

    public LocalizationService(ISettingsService settingsService)
    {
        // Load saved language or detect from system
        var savedLanguage = settingsService.Settings.Theme; // Reusing Theme field temporarily
        if (string.IsNullOrEmpty(savedLanguage) || savedLanguage == "Dark")
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
    }

    public void SetLanguage(string languageCode)
    {
        if (!_languageNames.ContainsKey(languageCode))
            return;

        if (CurrentLanguage == languageCode)
            return;

        CurrentLanguage = languageCode;
        LoadLanguageResources(languageCode);
        
        LanguageChanged?.Invoke(this, EventArgs.Empty);
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
            var resourcePath = $"Resources/Strings.{languageCode}.xaml";
            var uri = new Uri(resourcePath, UriKind.Relative);
            var resourceDict = new ResourceDictionary { Source = uri };

            // Remove old language dictionaries
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
        catch
        {
            // If loading fails, continue with existing resources
        }
    }
}
