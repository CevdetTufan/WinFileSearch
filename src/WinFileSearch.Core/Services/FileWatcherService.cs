using WinFileSearch.Data.Repositories;

namespace WinFileSearch.Core.Services;

public class FileWatcherService(IFileRepository repository) : IFileWatcherService, IDisposable
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = [];
    private readonly IFileRepository _repository = repository;
    private bool _isWatching;
    private bool _disposed;

    public event EventHandler<FileSystemEventArgs>? FileCreated;
    public event EventHandler<FileSystemEventArgs>? FileDeleted;
    public event EventHandler<RenamedEventArgs>? FileRenamed;
    public event EventHandler<FileSystemEventArgs>? FileChanged;

    public bool IsWatching => _isWatching;

	public async Task StartWatchingAsync()
	{
		if (_isWatching)
			return;

		var folders = await _repository.GetIncludedFoldersAsync();

		foreach (var folder in folders.Where(f => Directory.Exists(f.Path)))
		{
			AddWatcher(folder.Path);
		}

		_isWatching = true;
	}

    public void StopWatching()
    {
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
        _isWatching = false;
    }

    public void AddWatcher(string folderPath)
    {
        if (_watchers.ContainsKey(folderPath))
            return;

        if (!Directory.Exists(folderPath))
            return;

        try
        {
            var watcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName 
                             | NotifyFilters.DirectoryName 
                             | NotifyFilters.LastWrite 
                             | NotifyFilters.Size,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Created += OnFileCreated;
            watcher.Deleted += OnFileDeleted;
            watcher.Renamed += OnFileRenamed;
            watcher.Changed += OnFileChanged;
            watcher.Error += OnWatcherError;

            _watchers[folderPath] = watcher;
        }
        catch (Exception)
        {
            // Ignore folders that can't be watched
        }
    }

    public void RemoveWatcher(string folderPath)
    {
        if (_watchers.TryGetValue(folderPath, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            _watchers.Remove(folderPath);
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        // Only track files, not directories
        if (Directory.Exists(e.FullPath))
            return;

        FileCreated?.Invoke(this, e);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        FileDeleted?.Invoke(this, e);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        // Only track files, not directories
        if (Directory.Exists(e.FullPath))
            return;

        FileRenamed?.Invoke(this, e);
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Only track files, not directories
        if (Directory.Exists(e.FullPath))
            return;

        FileChanged?.Invoke(this, e);
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        // Log or handle watcher errors
        // In production, you might want to restart the watcher
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
            StopWatching();
        }

        _disposed = true;
    }
}
