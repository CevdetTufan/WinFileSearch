namespace WinFileSearch.Core.Services;

/// <summary>
/// Service for monitoring file system changes in indexed folders.
/// Uses FileSystemWatcher to detect file creation, deletion, renaming, and modifications.
/// </summary>
public interface IFileWatcherService
{
    /// <summary>
    /// Starts watching all indexed folders for file system changes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartWatchingAsync();

    /// <summary>
    /// Stops all active file system watchers.
    /// </summary>
    void StopWatching();

    /// <summary>
    /// Adds a new folder to the watch list.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to monitor.</param>
    void AddWatcher(string folderPath);

    /// <summary>
    /// Removes a folder from the watch list and disposes its watcher.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to stop monitoring.</param>
    void RemoveWatcher(string folderPath);

    /// <summary>
    /// Occurs when a new file is created in a watched folder.
    /// </summary>
    event EventHandler<FileSystemEventArgs>? FileCreated;

    /// <summary>
    /// Occurs when a file is deleted from a watched folder.
    /// </summary>
    event EventHandler<FileSystemEventArgs>? FileDeleted;

    /// <summary>
    /// Occurs when a file is renamed in a watched folder.
    /// </summary>
    event EventHandler<RenamedEventArgs>? FileRenamed;

    /// <summary>
    /// Occurs when a file's content or attributes are modified in a watched folder.
    /// </summary>
    event EventHandler<FileSystemEventArgs>? FileChanged;

    /// <summary>
    /// Gets a value indicating whether the watcher is currently monitoring folders.
    /// </summary>
    bool IsWatching { get; }
}
