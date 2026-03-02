using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinFileSearch.Core.Services;
using WinFileSearch.Data.Models;

namespace WinFileSearch.UI.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly IFileSearchService _searchService;

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
    }

    partial void OnSearchQueryChanged(string value)
    {
        // Auto-search when query changes (with debounce ideally)
        if (value.Length >= 2)
        {
            _ = SearchAsync();
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
            _ = SearchAsync();
        }
    }

    partial void OnSelectedFileChanged(FileEntry? value)
    {
        ShowPreview = value != null;
    }

    [RelayCommand]
    private async Task SearchAsync()
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
}
