using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using WinFileSearch.Core.Services;
using WinFileSearch.Data.Models;
using WinFileSearch.UI.Services;
using Timer = System.Timers.Timer;

namespace WinFileSearch.UI.ViewModels;

public partial class SearchViewModel : ObservableObject, IDisposable
{
    private readonly IFileSearchService _searchService;
    private readonly ISearchHistoryService _historyService;
    private readonly IFavoritesService _favoritesService;
    private readonly ILoggingService _loggingService;
    private readonly Timer _debounceTimer;
    private const int DebounceDelayMs = 300;
    private bool _disposed;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllSelected))]
    [NotifyPropertyChangedFor(nameof(IsDocumentsSelected))]
    [NotifyPropertyChangedFor(nameof(IsImagesSelected))]
    [NotifyPropertyChangedFor(nameof(IsMediaSelected))]
    private FileCategory? _selectedCategory;

    // Computed properties for filter toggle synchronization
    public bool IsAllSelected => SelectedCategory == null;
    public bool IsDocumentsSelected => SelectedCategory == FileCategory.Document;
    public bool IsImagesSelected => SelectedCategory == FileCategory.Image;
    public bool IsMediaSelected => SelectedCategory == FileCategory.Media;

    [ObservableProperty]
    private FileEntry? _selectedFile;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _showPreview;

    [ObservableProperty]
    private bool _showHistory;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSelectedFileFavorite;

    [ObservableProperty]
    private bool _showEmptyState;

    [ObservableProperty]
    private bool _hasSearched;

    public ObservableCollection<FileEntry> SearchResults { get; } = [];
    public ObservableCollection<string> SearchHistory { get; } = [];

    public SearchViewModel(IFileSearchService searchService, ISearchHistoryService historyService, IFavoritesService favoritesService, ILoggingService loggingService)
    {
        _searchService = searchService;
        _historyService = historyService;
        _favoritesService = favoritesService;
        _loggingService = loggingService;

        // Initialize debounce timer
        _debounceTimer = new Timer(DebounceDelayMs);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += OnDebounceTimerElapsed;

        // Load search history
        LoadHistory();
        _historyService.HistoryChanged += OnHistoryChanged;
        _favoritesService.FavoritesChanged += OnFavoritesChanged;

        _loggingService.LogDebug("SearchViewModel initialized");
    }

    private void OnHistoryChanged(object? sender, EventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(LoadHistory);
    }

    private void OnFavoritesChanged(object? sender, EventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(UpdateFavoriteStatus);
    }

    private void UpdateFavoriteStatus()
    {
        IsSelectedFileFavorite = SelectedFile != null && _favoritesService.IsFavorite(SelectedFile.FullPath);
    }

    private void LoadHistory()
    {
        SearchHistory.Clear();
        foreach (var item in _historyService.GetHistory())
        {
            SearchHistory.Add(item);
        }
    }

    private void OnDebounceTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Execute search on UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
        {
            await ExecuteSearchAsync();
        });
    }

    partial void OnSearchQueryChanged(string value)
    {
        // Reset and start debounce timer
        _debounceTimer.Stop();

        if (value.Length >= 2)
        {
            StatusMessage = "Typing...";
            ShowHistory = false;
            _debounceTimer.Start();
        }
        else if (string.IsNullOrEmpty(value))
        {
            SearchResults.Clear();
            StatusMessage = string.Empty;
            ShowHistory = SearchHistory.Count > 0;
            ShowEmptyState = false;
            HasSearched = false;
        }
        else
        {
            ShowHistory = SearchHistory.Count > 0;
        }
    }

    partial void OnSelectedCategoryChanged(FileCategory? value)
    {
        if (!string.IsNullOrEmpty(SearchQuery))
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
    }

    partial void OnSelectedFileChanged(FileEntry? value)
    {
        ShowPreview = value != null;
        UpdateFavoriteStatus();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        _debounceTimer.Stop();
        await ExecuteSearchAsync();
    }

    [RelayCommand]
    private void SelectHistoryItem(string? query)
    {
        if (!string.IsNullOrEmpty(query))
        {
            ShowHistory = false;
            SearchQuery = query;
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        _historyService.ClearHistory();
        ShowHistory = false;
    }

    [RelayCommand]
    private void HideHistory()
    {
        ShowHistory = false;
    }

    private async Task ExecuteSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;

        IsSearching = true;
        ShowHistory = false;
        ShowEmptyState = false;
        StatusMessage = "Searching...";

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var filter = new SearchFilter
            {
                Query = SearchQuery,
                Category = SelectedCategory,
                MaxResults = 100
            };

            _loggingService.LogDebug("Executing search: Query={Query}, Category={Category}", SearchQuery, SelectedCategory?.ToString() ?? "All");

            var results = await _searchService.SearchAsync(filter);

            SearchResults.Clear();
            foreach (var file in results)
            {
                SearchResults.Add(file);
            }

            stopwatch.Stop();
            var duration = stopwatch.Elapsed;
            _loggingService.LogPerformance($"Search '{SearchQuery}'", duration);

            HasSearched = true;
            ShowEmptyState = SearchResults.Count == 0;
            StatusMessage = SearchResults.Count == 0 
                ? "No files found" 
                : $"{SearchResults.Count} files found";

            // Add to history only if we got results
            if (SearchResults.Count > 0)
            {
                _historyService.AddSearch(SearchQuery);
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Search failed for query: {Query}", ex, SearchQuery);
            StatusMessage = $"Search error: {ex.Message}";
            ShowEmptyState = true;
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void ClearCategory()
    {
        SelectedCategory = null;
    }

    [RelayCommand]
    private void SetCategoryAll()
    {
        SelectedCategory = null;
    }

    [RelayCommand]
    private void SetCategoryDocuments()
    {
        SelectedCategory = FileCategory.Document;
    }

    [RelayCommand]
    private void SetCategoryImages()
    {
        SelectedCategory = FileCategory.Image;
    }

    [RelayCommand]
    private void SetCategoryMedia()
    {
        SelectedCategory = FileCategory.Media;
    }

    [RelayCommand]
    private void OpenSelectedFile()
    {
        if (SelectedFile != null)
        {
            _searchService.OpenFile(SelectedFile.FullPath);
        }
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        if (SelectedFile != null)
        {
            _searchService.OpenFileLocation(SelectedFile.FullPath);
        }
    }

    [RelayCommand]
    private void CopyPath()
    {
        if (SelectedFile != null)
        {
            System.Windows.Clipboard.SetText(SelectedFile.FullPath);
        }
    }

    [RelayCommand]
    private void CopyFileName()
    {
        if (SelectedFile != null)
        {
            System.Windows.Clipboard.SetText(SelectedFile.FileName);
        }
    }

    [RelayCommand]
    private void ClosePreview()
    {
        SelectedFile = null;
        ShowPreview = false;
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (SelectedFile != null)
        {
            _favoritesService.ToggleFavorite(SelectedFile.FullPath);
        }
    }

    [RelayCommand]
    private void ToggleFavoriteForFile(FileEntry? file)
    {
        if (file != null)
        {
            _favoritesService.ToggleFavorite(file.FullPath);
            // Update UI if it's the selected file
            if (SelectedFile?.FullPath == file.FullPath)
            {
                UpdateFavoriteStatus();
            }
        }
    }

    public bool IsFavorite(string filePath)
    {
        return _favoritesService.IsFavorite(filePath);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
            _debounceTimer.Stop();
            _debounceTimer.Elapsed -= OnDebounceTimerElapsed;
            _debounceTimer.Dispose();
            _historyService.HistoryChanged -= OnHistoryChanged;
            _favoritesService.FavoritesChanged -= OnFavoritesChanged;
        }

        _disposed = true;
    }
}
