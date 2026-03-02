namespace WinFileSearch.Core.Services;

public interface IFileWatcherService
{
    /// <summary>
    /// Starts watching all indexed folders for changes
    /// </summary>
    Task StartWatchingAsync();
    
    /// <summary>
    /// Stops watching all folders
    /// </summary>
    void StopWatching();
    
    /// <summary>
    /// Adds a folder to watch
    /// </summary>
    void AddWatcher(string folderPath);
    
    /// <summary>
    /// Removes a folder watcher
    /// </summary>
    void RemoveWatcher(string folderPath);
    
    /// <summary>
    /// Event raised when a file is created
    /// </summary>
    event EventHandler<FileSystemEventArgs>? FileCreated;
    
    /// <summary>
    /// Event raised when a file is deleted
    /// </summary>
    event EventHandler<FileSystemEventArgs>? FileDeleted;
    
    /// <summary>
    /// Event raised when a file is renamed
    /// </summary>
    event EventHandler<RenamedEventArgs>? FileRenamed;
    
    /// <summary>
    /// Event raised when a file is changed
    /// </summary>
    event EventHandler<FileSystemEventArgs>? FileChanged;
    
    /// <summary>
    /// Gets whether the watcher is currently active
    /// </summary>
    bool IsWatching { get; }
}
