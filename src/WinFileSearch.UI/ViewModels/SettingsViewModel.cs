using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using WinFileSearch.Core.Services;
using WinFileSearch.Data.Models;

namespace WinFileSearch.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IFileIndexService _indexService;
    private CancellationTokenSource? _indexingCts;

    [ObservableProperty]
    private bool _backgroundIndexingEnabled;

    [ObservableProperty]
    private bool _isIndexing;

    [ObservableProperty]
    private double _indexingProgress;

    [ObservableProperty]
    private string _indexingStatus = string.Empty;

    [ObservableProperty]
    private long _totalFilesIndexed;

    [ObservableProperty]
    private DateTime? _lastIndexed;

    public ObservableCollection<IndexedFolder> IncludedFolders { get; } = new();
    public ObservableCollection<IndexedFolder> ExcludedFolders { get; } = new();

    public SettingsViewModel(IFileIndexService indexService)
    {
        _indexService = indexService;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var includedFolders = await _indexService.GetIncludedFoldersAsync();
        var excludedFolders = await _indexService.GetExcludedFoldersAsync();

        IncludedFolders.Clear();
        foreach (var folder in includedFolders)
        {
            IncludedFolders.Add(folder);
        }

        ExcludedFolders.Clear();
        foreach (var folder in excludedFolders)
        {
            ExcludedFolders.Add(folder);
        }

        TotalFilesIndexed = await _indexService.GetTotalFileCountAsync();
        LastIndexed = await _indexService.GetLastIndexTimeAsync();
    }

    [RelayCommand]
    private async Task AddFolderAsync()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select a folder to index",
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            await AddFolderByPathAsync(dialog.SelectedPath);
        }
    }

    /// <summary>
    /// Adds a folder by path (used by drag & drop)
    /// </summary>
    public async void AddFolderByPath(string path)
    {
        await AddFolderByPathAsync(path);
    }

    private async Task AddFolderByPathAsync(string path)
    {
        // Check if folder is already in the list
        if (IncludedFolders.Any(f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var folder = await _indexService.AddFolderAsync(path);
        IncludedFolders.Add(folder);

        // Start indexing the new folder
        await IndexFolderAsync(folder.Path);
    }

    [RelayCommand]
    private async Task RemoveFolderAsync(IndexedFolder folder)
    {
        if (folder == null) return;

        await _indexService.RemoveFolderAsync(folder.Id);
        IncludedFolders.Remove(folder);
        TotalFilesIndexed = await _indexService.GetTotalFileCountAsync();
    }

    [RelayCommand]
    private async Task AddExclusionAsync()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select a folder to exclude",
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            await _indexService.AddExcludedFolderAsync(dialog.SelectedPath);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RemoveExclusionAsync(IndexedFolder folder)
    {
        if (folder == null) return;

        await _indexService.RemoveExcludedFolderAsync(folder.Id);
        ExcludedFolders.Remove(folder);
    }

    [RelayCommand]
    private async Task RebuildIndexAsync()
    {
        if (IsIndexing)
        {
            // Cancel current indexing
            _indexingCts?.Cancel();
            return;
        }

        IsIndexing = true;
        IndexingProgress = 0;
        IndexingStatus = "Rebuilding index...";
        _indexingCts = new CancellationTokenSource();

        var progress = new Progress<IndexingProgress>(p =>
        {
            IndexingProgress = p.PercentComplete;
            IndexingStatus = $"Indexing: {p.CurrentFile}";
            
            if (p.IsCompleted)
            {
                IndexingStatus = "Indexing completed";
            }
            else if (p.IsCancelled)
            {
                IndexingStatus = "Indexing cancelled";
            }
        });

        try
        {
            await _indexService.RebuildIndexAsync(progress, _indexingCts.Token);
            await LoadDataAsync();
        }
        catch (OperationCanceledException)
        {
            IndexingStatus = "Indexing cancelled";
        }
        finally
        {
            IsIndexing = false;
            _indexingCts = null;
        }
    }

    private async Task IndexFolderAsync(string folderPath)
    {
        IsIndexing = true;
        IndexingProgress = 0;
        IndexingStatus = $"Indexing {folderPath}...";
        _indexingCts = new CancellationTokenSource();

        var progress = new Progress<IndexingProgress>(p =>
        {
            IndexingProgress = p.PercentComplete;
            IndexingStatus = $"Indexing: {p.CurrentFile} ({p.ProcessedFiles}/{p.TotalFiles})";
            
            if (p.IsCompleted)
            {
                IndexingStatus = "Indexing completed";
            }
        });

        try
        {
            await _indexService.IndexFolderAsync(folderPath, progress, _indexingCts.Token);
            await LoadDataAsync();
        }
        catch (OperationCanceledException)
        {
            IndexingStatus = "Indexing cancelled";
        }
        finally
        {
            IsIndexing = false;
            _indexingCts = null;
        }
    }

    [RelayCommand]
    private void CancelIndexing()
    {
        _indexingCts?.Cancel();
    }
}
