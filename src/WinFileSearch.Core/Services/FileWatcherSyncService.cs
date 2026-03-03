using WinFileSearch.Data.Models;
using WinFileSearch.Data.Repositories;

namespace WinFileSearch.Core.Services;

/// <summary>
/// Handles FileWatcher events and synchronizes with database
/// </summary>
public class FileWatcherSyncService : IDisposable
{
    private readonly IFileWatcherService _watcherService;
    private readonly IFileRepository _repository;
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    
    public event EventHandler<string>? FileSynced;

    public FileWatcherSyncService(IFileWatcherService watcherService, IFileRepository repository)
    {
        _watcherService = watcherService;
        _repository = repository;

        // Subscribe to watcher events
        _watcherService.FileCreated += OnFileCreated;
        _watcherService.FileDeleted += OnFileDeleted;
        _watcherService.FileRenamed += OnFileRenamed;
        _watcherService.FileChanged += OnFileChanged;
    }

    private async void OnFileCreated(object? sender, FileSystemEventArgs e)
    {
        await _syncLock.WaitAsync();
        try
        {
            if (!File.Exists(e.FullPath))
                return;

            var fileInfo = new FileInfo(e.FullPath);
            var extension = fileInfo.Extension.ToLowerInvariant();

            // Find the parent indexed folder
            var folders = await _repository.GetIncludedFoldersAsync();
            var parentFolder = folders.FirstOrDefault(f => 
                e.FullPath.StartsWith(f.Path, StringComparison.OrdinalIgnoreCase));

            if (parentFolder == null)
                return;

            // Check if in excluded folder
            var excludedFolders = await _repository.GetExcludedFoldersAsync();
            var directory = fileInfo.DirectoryName ?? "";
            if (excludedFolders.Any(ef => directory.StartsWith(ef.Path, StringComparison.OrdinalIgnoreCase)))
                return;

            var fileEntry = new FileEntry
            {
                FileName = fileInfo.Name,
                FullPath = fileInfo.FullName,
                Extension = extension,
                Directory = fileInfo.DirectoryName ?? "",
                Size = fileInfo.Length,
                CreatedAt = fileInfo.CreationTime,
                ModifiedAt = fileInfo.LastWriteTime,
                FolderId = parentFolder.Id,
                Category = FileCategoryHelper.GetCategory(extension)
            };

            await _repository.InsertFileAsync(fileEntry);
            FileSynced?.Invoke(this, $"Added: {fileInfo.Name}");
        }
        catch (Exception)
        {
            // Silently handle errors for background sync
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async void OnFileDeleted(object? sender, FileSystemEventArgs e)
    {
        await _syncLock.WaitAsync();
        try
        {
            await _repository.DeleteFileByPathAsync(e.FullPath);
            FileSynced?.Invoke(this, $"Removed: {Path.GetFileName(e.FullPath)}");
        }
        catch (Exception)
        {
            // Silently handle errors
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async void OnFileRenamed(object? sender, RenamedEventArgs e)
    {
        await _syncLock.WaitAsync();
        try
        {
            // Delete old entry
            await _repository.DeleteFileByPathAsync(e.OldFullPath);

            // Add new entry if file exists
            if (!File.Exists(e.FullPath))
                return;

            var fileInfo = new FileInfo(e.FullPath);
            var extension = fileInfo.Extension.ToLowerInvariant();

            var folders = await _repository.GetIncludedFoldersAsync();
            var parentFolder = folders.FirstOrDefault(f => 
                e.FullPath.StartsWith(f.Path, StringComparison.OrdinalIgnoreCase));

            if (parentFolder == null)
                return;

            var fileEntry = new FileEntry
            {
                FileName = fileInfo.Name,
                FullPath = fileInfo.FullName,
                Extension = extension,
                Directory = fileInfo.DirectoryName ?? "",
                Size = fileInfo.Length,
                CreatedAt = fileInfo.CreationTime,
                ModifiedAt = fileInfo.LastWriteTime,
                FolderId = parentFolder.Id,
                Category = FileCategoryHelper.GetCategory(extension)
            };

            await _repository.InsertFileAsync(fileEntry);
            FileSynced?.Invoke(this, $"Renamed: {e.OldName} → {fileInfo.Name}");
        }
        catch (Exception)
        {
            // Silently handle errors
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async void OnFileChanged(object? sender, FileSystemEventArgs e)
    {
        await _syncLock.WaitAsync();
        try
        {
            if (!File.Exists(e.FullPath))
                return;

            var existingFile = await _repository.GetFileByPathAsync(e.FullPath);
            if (existingFile == null)
                return;

            var fileInfo = new FileInfo(e.FullPath);
            
            existingFile.Size = fileInfo.Length;
            existingFile.ModifiedAt = fileInfo.LastWriteTime;

            await _repository.UpdateFileAsync(existingFile);
            FileSynced?.Invoke(this, $"Updated: {fileInfo.Name}");
        }
        catch (Exception)
        {
            // Silently handle errors
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public void Dispose()
    {
        _watcherService.FileCreated -= OnFileCreated;
        _watcherService.FileDeleted -= OnFileDeleted;
        _watcherService.FileRenamed -= OnFileRenamed;
        _watcherService.FileChanged -= OnFileChanged;
        _syncLock.Dispose();
    }
}
