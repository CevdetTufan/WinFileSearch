using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using WinFileSearch.Core.Services;
using WinFileSearch.Data.Models;
using WinFileSearch.UI.Services;

namespace WinFileSearch.UI.ViewModels;

public class LanguageOption
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly IFileIndexService _indexService;
    private readonly IStartupService _startupService;
    private readonly ISettingsService _settingsService;
    private readonly IPerformanceMetricsService _metricsService;
    private readonly ILoggingService _loggingService;
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

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _minimizeToTray;

    // Performance metrics
    [ObservableProperty]
    private long _totalSearches;

    [ObservableProperty]
    private double _averageSearchTimeMs;

    [ObservableProperty]
    private long _memoryUsageMB;

    [ObservableProperty]
    private string _uptime = string.Empty;

    [ObservableProperty]
    private string _logFilePath = string.Empty;

    [ObservableProperty]
    private string _selectedLanguage = "en";

    public ObservableCollection<IndexedFolder> IncludedFolders { get; } = new();
    public ObservableCollection<IndexedFolder> ExcludedFolders { get; } = new();
    public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new()
    {
        new LanguageOption { Code = "en", Name = "English" },
        new LanguageOption { Code = "tr", Name = "Türkçe" }
    };

    public SettingsViewModel(IFileIndexService indexService, IStartupService startupService, ISettingsService settingsService, IPerformanceMetricsService metricsService, ILoggingService loggingService)
    {
        _indexService = indexService;
        _startupService = startupService;
        _settingsService = settingsService;
        _metricsService = metricsService;
        _loggingService = loggingService;

        // Load settings
        StartWithWindows = _startupService.IsStartupEnabled;
        MinimizeToTray = _settingsService.Settings.MinimizeToTray;
        BackgroundIndexingEnabled = _settingsService.Settings.BackgroundIndexing;
        LogFilePath = _loggingService.GetLogFilePath();

        _ = LoadDataAsync();
        RefreshMetrics();
    }

    [RelayCommand]
    private void RefreshMetrics()
    {
        var metrics = _metricsService.GetMetrics();
        TotalSearches = metrics.TotalSearches;
        AverageSearchTimeMs = Math.Round(metrics.AverageSearchTimeMs, 2);
        MemoryUsageMB = metrics.MemoryUsageMB;
        Uptime = FormatUptime(metrics.Uptime);
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        return $"{uptime.Minutes}m {uptime.Seconds}s";
    }

    [RelayCommand]
    private void OpenLogFolder()
    {
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", LogFilePath);
        }
        catch { }
    }

    partial void OnStartWithWindowsChanged(bool value)
    {
        _startupService.SetStartup(value);
        _settingsService.Settings.StartWithWindows = value;
        _settingsService.Save();
    }

    partial void OnMinimizeToTrayChanged(bool value)
    {
        _settingsService.Settings.MinimizeToTray = value;
        _settingsService.Save();
    }

    partial void OnBackgroundIndexingEnabledChanged(bool value)
    {
        _settingsService.Settings.BackgroundIndexing = value;
        _settingsService.Save();
    }

    private async Task LoadDataAsync()
    {
        // Run database operations on background thread to prevent UI freeze
        var includedFolders = await Task.Run(() => _indexService.GetIncludedFoldersAsync());
        var excludedFolders = await Task.Run(() => _indexService.GetExcludedFoldersAsync());

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

        TotalFilesIndexed = await Task.Run(() => _indexService.GetTotalFileCountAsync());
        LastIndexed = await Task.Run(() => _indexService.GetLastIndexTimeAsync());
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

        // Run on background thread to prevent UI freeze
        var folder = await Task.Run(() => _indexService.AddFolderAsync(path));
        IncludedFolders.Add(folder);

        // Start indexing the new folder
        await IndexFolderAsync(folder.Path);
    }

    [RelayCommand]
    private async Task RemoveFolderAsync(IndexedFolder folder)
    {
        if (folder == null) return;

        // Run on background thread to prevent UI freeze during deletion
        await Task.Run(() => _indexService.RemoveFolderAsync(folder.Id));
        IncludedFolders.Remove(folder);
        TotalFilesIndexed = await Task.Run(() => _indexService.GetTotalFileCountAsync());
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
            // Run on background thread to prevent UI freeze
            await Task.Run(() => _indexService.AddExcludedFolderAsync(dialog.SelectedPath));
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RemoveExclusionAsync(IndexedFolder folder)
    {
        if (folder == null) return;

        // Run on background thread to prevent UI freeze
        await Task.Run(() => _indexService.RemoveExcludedFolderAsync(folder.Id));
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
            // Run indexing on background thread to prevent UI freeze
            await Task.Run(async () =>
            {
                await _indexService.RebuildIndexAsync(progress, _indexingCts.Token);
            }, _indexingCts.Token);
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
            // Run indexing on background thread to prevent UI freeze
            await Task.Run(async () =>
            {
                await _indexService.IndexFolderAsync(folderPath, progress, _indexingCts.Token);
            }, _indexingCts.Token);
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
