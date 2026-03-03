using System.IO;
using System.Text.Json;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for managing favorite/starred files
/// </summary>
public interface IFavoritesService
{
    IReadOnlyList<string> GetFavorites();
    bool IsFavorite(string filePath);
    void AddFavorite(string filePath);
    void RemoveFavorite(string filePath);
    void ToggleFavorite(string filePath);
    event EventHandler? FavoritesChanged;
}

public class FavoritesService : IFavoritesService
{
    private readonly HashSet<string> _favorites = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _favoritesFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public event EventHandler? FavoritesChanged;

    public FavoritesService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinFileSearch");

        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _favoritesFilePath = Path.Combine(appDataPath, "favorites.json");
        LoadFavorites();
    }

    public IReadOnlyList<string> GetFavorites() => _favorites.ToList().AsReadOnly();

    public bool IsFavorite(string filePath)
    {
        return !string.IsNullOrEmpty(filePath) && _favorites.Contains(filePath);
    }

    public void AddFavorite(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        if (_favorites.Add(filePath))
        {
            SaveFavorites();
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RemoveFavorite(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        if (_favorites.Remove(filePath))
        {
            SaveFavorites();
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ToggleFavorite(string filePath)
    {
        if (IsFavorite(filePath))
            RemoveFavorite(filePath);
        else
            AddFavorite(filePath);
    }

    private void LoadFavorites()
    {
        try
        {
            if (File.Exists(_favoritesFilePath))
            {
                var json = File.ReadAllText(_favoritesFilePath);
                var favorites = JsonSerializer.Deserialize<List<string>>(json);
                if (favorites != null)
                {
                    foreach (var path in favorites)
                    {
                        _favorites.Add(path);
                    }
                }
            }
        }
        catch
        {
            // If loading fails, start with empty favorites
        }
    }

    private void SaveFavorites()
    {
        try
        {
            var json = JsonSerializer.Serialize(_favorites.ToList(), JsonOptions);
            File.WriteAllText(_favoritesFilePath, json);
        }
        catch
        {
            // Silently fail if saving doesn't work
        }
    }
}
