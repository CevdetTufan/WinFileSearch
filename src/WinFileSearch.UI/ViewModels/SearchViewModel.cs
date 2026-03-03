using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Timers;
using WinFileSearch.Core.Services;
using WinFileSearch.Data.Models;
using Timer = System.Timers.Timer;

namespace WinFileSearch.UI.ViewModels;

public partial class SearchViewModel : ObservableObject, IDisposable
{
    private readonly IFileSearchService _searchService;
    private readonly Timer _debounceTimer;
    private const int DebounceDelayMs = 300;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private FileCategory? _selectedCategory;

    [ObservableProperty]
    private FileEntry? _selectedFile;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _showPreview;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<FileEntry> SearchResults { get; } = new();

    public SearchViewModel(IFileSearchService searchService)
    {
        _searchService = searchService;

        // Initialize debounce timer
        _debounceTimer = new Timer(DebounceDelayMs);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += OnDebounceTimerElapsed;
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
            _debounceTimer.Start();
        }
        else if (string.IsNullOrEmpty(value))
        {
            SearchResults.Clear();
            StatusMessage = string.Empty;
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
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        _debounceTimer.Stop();
        await ExecuteSearchAsync();
    }

    private async Task ExecuteSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;

        IsSearching = true;
        StatusMessage = "Searching...";

        try
        {
            var filter = new SearchFilter
            {
                Query = SearchQuery,
                Category = SelectedCategory,
                MaxResults = 100
            };

            var results = await _searchService.SearchAsync(filter);

            SearchResults.Clear();
            foreach (var file in results)
            {
                SearchResults.Add(file);
            }

            StatusMessage = $"{SearchResults.Count} files found";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
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
    private void ClosePreview()
    {
        SelectedFile = null;
        ShowPreview = false;
    }

    public void Dispose()
    {
        _debounceTimer.Stop();
        _debounceTimer.Elapsed -= OnDebounceTimerElapsed;
        _debounceTimer.Dispose();
    }
}
