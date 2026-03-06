using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinFileSearch.Core.Services;
using WinFileSearch.Data.Models;
using WinFileSearch.UI.Services;

namespace WinFileSearch.UI.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IFileSearchService _searchService;
    private readonly IFileIndexService _indexService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private FileCategory? _selectedCategory;

    [ObservableProperty]
    private bool _isIndexing;

    [ObservableProperty]
    private double _indexingProgress;

    [ObservableProperty]
    private string _indexingStatus = string.Empty;

    [ObservableProperty]
    private long _totalFilesIndexed;

    public ObservableCollection<FileEntry> RecentFiles { get; } = [];

    public HomeViewModel(IFileSearchService searchService, IFileIndexService indexService, INavigationService navigationService)
    {
        _searchService = searchService;
        _indexService = indexService;
        _navigationService = navigationService;
        _ = LoadRecentFilesAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        // Navigate to search page when user starts typing
        if (!string.IsNullOrEmpty(value) && value.Length >= 2)
        {
            _navigationService.NavigateToSearch(value);
            SearchQuery = string.Empty; // Clear after navigation
        }
    }

    public async Task LoadRecentFilesAsync()
    {
        var recentFiles = await _searchService.GetRecentFilesAsync(10);

        RecentFiles.Clear();
        foreach (var file in recentFiles)
        {
            RecentFiles.Add(file);
        }

        TotalFilesIndexed = await _indexService.GetTotalFileCountAsync();
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
    private void OpenFile(FileEntry? file)
    {
        if (file != null)
        {
            _searchService.OpenFile(file.FullPath);
        }
    }

    [RelayCommand]
    private void OpenFileLocation(FileEntry? file)
    {
        if (file != null)
        {
            _searchService.OpenFileLocation(file.FullPath);
        }
    }

    public void UpdateIndexingProgress(IndexingProgress progress)
    {
        IsIndexing = !progress.IsCompleted && !progress.IsCancelled;
        IndexingProgress = progress.PercentComplete;
        
        if (progress.IsCompleted)
        {
            IndexingStatus = "Indexing complete";
            _ = LoadRecentFilesAsync();
        }
        else if (progress.IsCancelled)
        {
            IndexingStatus = "Indexing cancelled";
        }
        else
        {
            IndexingStatus = $"Indexing {progress.PercentComplete:F0}% complete...";
        }
    }
}
