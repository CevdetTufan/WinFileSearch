using System.IO;
using System.Text.Json;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for managing search history
/// </summary>
public interface ISearchHistoryService
{
    IReadOnlyList<string> GetHistory();
    void AddSearch(string query);
    void ClearHistory();
    event EventHandler? HistoryChanged;
}

public class SearchHistoryService : ISearchHistoryService
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };
    private readonly List<string> _history = [];
    private readonly string _historyFilePath;
    private const int MaxHistoryItems = 10;

    public event EventHandler? HistoryChanged;

    public SearchHistoryService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinFileSearch");
        
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }
        
        _historyFilePath = Path.Combine(appDataPath, "search_history.json");
        LoadHistory();
    }

    public IReadOnlyList<string> GetHistory() => _history.AsReadOnly();

    public void AddSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return;

        query = query.Trim();

        // Remove if already exists (to move to top)
        _history.Remove(query);
        
        // Add to beginning
        _history.Insert(0, query);

        // Limit to max items
        while (_history.Count > MaxHistoryItems)
        {
            _history.RemoveAt(_history.Count - 1);
        }

        SaveHistory();
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearHistory()
    {
        _history.Clear();
        SaveHistory();
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void LoadHistory()
    {
        try
        {
            if (File.Exists(_historyFilePath))
            {
                var json = File.ReadAllText(_historyFilePath);
                var history = JsonSerializer.Deserialize<List<string>>(json);
                if (history != null)
                {
                    _history.AddRange(history.Take(MaxHistoryItems));
                }
            }
        }
        catch (Exception)
        {
            // Ignore load errors
        }
    }

    private void SaveHistory()
    {
        try
        {
            var json = JsonSerializer.Serialize(_history, s_jsonOptions);
            File.WriteAllText(_historyFilePath, json);
        }
        catch (Exception)
        {
            // Ignore save errors
        }
    }
}
