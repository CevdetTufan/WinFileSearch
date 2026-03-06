using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinFileSearch.Core.Interfaces;
using WinFileSearch.Core.Models;
using WinFileSearch.Core.Services;
using WinFileSearch.Data.Models;
using WinFileSearch.UI.Services;

namespace WinFileSearch.UI.ViewModels;

public class LanguageOption
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public partial class SettingsViewModel : ObservableObject, IDisposable
{
    private readonly IFileIndexService _indexService;
    private readonly IStartupService _startupService;
    private readonly ISettingsService _settingsService;
    private readonly IPerformanceMetricsService _metricsService;
    private readonly ILoggingService _loggingService;
    private readonly IUpdateService _updateService;
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

    // Update properties
    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string _currentVersion = string.Empty;

    [ObservableProperty]
    private string _latestVersion = string.Empty;

    [ObservableProperty]
    private string _updateReleaseNotes = string.Empty;

    [ObservableProperty]
    private bool _isDownloadingUpdate;

    [ObservableProperty]
    private int _downloadProgress;

    private UpdateInfo? _updateInfo;

    public ObservableCollection<IndexedFolder> IncludedFolders { get; } = new();
    public ObservableCollection<IndexedFolder> ExcludedFolders { get; } = new();
    public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new()
    {
        new LanguageOption { Code = "en", Name = "English" },
        new LanguageOption { Code = "tr", Name = "Türkçe" }
    };

    public SettingsViewModel(
        IFileIndexService indexService, 
        IStartupService startupService, 
        ISettingsService settingsService, 
        IPerformanceMetricsService metricsService, 
        ILoggingService loggingService,
        IUpdateService updateService)
    {
        _indexService = indexService;
        _startupService = startupService;
        _settingsService = settingsService;
        _metricsService = metricsService;
        _loggingService = loggingService;
        _updateService = updateService;

        // Load settings
        StartWithWindows = _startupService.IsStartupEnabled;
        MinimizeToTray = _settingsService.Settings.MinimizeToTray;
        BackgroundIndexingEnabled = _settingsService.Settings.BackgroundIndexing;
        LogFilePath = _loggingService.GetLogFilePath();
        CurrentVersion = $"v{_updateService.CurrentVersion}";

        _ = LoadDataAsync();
        RefreshMetrics();
        _ = CheckForUpdatesAsync();
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
            if (_indexingCts != null)
            {
                await _indexingCts.CancelAsync();
            }
            return;
        }

        IsIndexing = true;
        IndexingProgress = 0;
        IndexingStatus = "Rebuilding index...";
        _indexingCts?.Dispose();
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
            _indexingCts?.Dispose();
            _indexingCts = null;
        }
    }

    private async Task IndexFolderAsync(string folderPath)
    {
        IsIndexing = true;
        IndexingProgress = 0;
        IndexingStatus = $"Indexing {folderPath}...";
        _indexingCts?.Dispose();
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
            _indexingCts?.Dispose();
            _indexingCts = null;
        }
    }

    [RelayCommand]
    private async Task CancelIndexingAsync()
    {
        if (_indexingCts != null)
        {
            await _indexingCts.CancelAsync();
        }
    }

    #region Update Methods

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            // Check if update info was already fetched at startup
            if (System.Windows.Application.Current.Properties["UpdateAvailable"] is UpdateInfo cachedUpdate)
            {
                _updateInfo = cachedUpdate;
                IsUpdateAvailable = true;
                LatestVersion = $"v{cachedUpdate.Version}";
                UpdateReleaseNotes = cachedUpdate.ReleaseNotes;
                System.Windows.Application.Current.Properties.Remove("UpdateAvailable");
                return;
            }

            var updateInfo = await Task.Run(() => _updateService.CheckForUpdateAsync());

            if (updateInfo != null)
            {
                _updateInfo = updateInfo;
                IsUpdateAvailable = true;
                LatestVersion = $"v{updateInfo.Version}";
                UpdateReleaseNotes = updateInfo.ReleaseNotes;
            }
            else
            {
                IsUpdateAvailable = false;
            }
        }
        catch
        {
            IsUpdateAvailable = false;
        }
    }

    [RelayCommand]
    private void OpenReleasePage()
    {
        if (_updateInfo != null)
        {
            _updateService.OpenReleasePage(_updateInfo.ReleasePageUrl);
        }
    }

    [RelayCommand]
    private async Task DownloadAndInstallUpdateAsync()
    {
        if (_updateInfo == null || string.IsNullOrEmpty(_updateInfo.DownloadUrl))
            return;

        try
        {
            IsDownloadingUpdate = true;
            DownloadProgress = 0;

            var progress = new Progress<int>(p => DownloadProgress = p);
            await _updateService.DownloadAndInstallAsync(_updateInfo.DownloadUrl, progress);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Failed to download update", ex);
            IsDownloadingUpdate = false;

            // Show error and offer to open release page
            _updateService.OpenReleasePage(_updateInfo.ReleasePageUrl);
        }
    }

    #endregion

    public void Dispose()
    {
        _indexingCts?.Dispose();
    }
}
