using WinFileSearch.Data;
using WinFileSearch.Data.Models;
using WinFileSearch.Data.Repositories;

namespace WinFileSearch.Core.Services;

public class FileIndexService : IFileIndexService
{
    private readonly IFileRepository _repository;
    private readonly FileSearchDbContext _dbContext;
    private const int BatchSize = 500;

    public FileIndexService(IFileRepository repository, FileSearchDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    public async Task IndexFolderAsync(string folderPath, IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

        // Get or create folder entry
        var folder = await _repository.GetFolderByPathAsync(folderPath);
        if (folder == null)
        {
            folder = new IndexedFolder
            {
                Path = folderPath,
                LastIndexed = DateTime.Now
            };
            folder.Id = await _repository.InsertFolderAsync(folder);
        }

        // Get excluded folders for filtering
        var excludedFolders = (await _repository.GetExcludedFoldersAsync()).ToList();

        var progressInfo = new IndexingProgress
        {
            CurrentFolder = folderPath
        };

        // Count total files first
        var allFiles = new List<string>();
        try
        {
            allFiles = Directory.EnumerateFiles(folderPath, "*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            }).ToList();
        }
        catch (Exception)
        {
            // Ignore access errors
        }

        progressInfo.TotalFiles = allFiles.Count;
        progress?.Report(progressInfo);

        var batch = new List<FileEntry>();
        long totalSize = 0;
        int fileCount = 0;

        foreach (var filePath in allFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                progressInfo.IsCancelled = true;
                progress?.Report(progressInfo);
                return;
            }

            // Skip excluded folders
            var directory = Path.GetDirectoryName(filePath) ?? "";
            if (excludedFolders.Any(ef => directory.StartsWith(ef.Path, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            try
            {
                var fileInfo = new FileInfo(filePath);
                var extension = fileInfo.Extension.ToLowerInvariant();

                var fileEntry = new FileEntry
                {
                    FileName = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    Extension = extension,
                    Directory = fileInfo.DirectoryName ?? "",
                    Size = fileInfo.Length,
                    CreatedAt = fileInfo.CreationTime,
                    ModifiedAt = fileInfo.LastWriteTime,
                    FolderId = folder.Id,
                    Category = FileCategoryHelper.GetCategory(extension)
                };

                batch.Add(fileEntry);
                totalSize += fileInfo.Length;
                fileCount++;

                progressInfo.ProcessedFiles++;
                progressInfo.CurrentFile = fileInfo.Name;

                // Insert in batches
                if (batch.Count >= BatchSize)
                {
                    await _repository.InsertFilesAsync(batch);
                    batch.Clear();
                    progress?.Report(progressInfo);
                }
            }
            catch (Exception)
            {
                // Skip files that can't be accessed
            }
        }

        // Insert remaining files
        if (batch.Count > 0)
        {
            await _repository.InsertFilesAsync(batch);
        }

        // Update folder statistics
        folder.LastIndexed = DateTime.Now;
        folder.FileCount = fileCount;
        folder.TotalSize = totalSize;
        await _repository.UpdateFolderAsync(folder);

        progressInfo.IsCompleted = true;
        progress?.Report(progressInfo);
    }

    public async Task IndexAllFoldersAsync(IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var folders = await _repository.GetIncludedFoldersAsync();
        
        foreach (var folder in folders)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await IndexFolderAsync(folder.Path, progress, cancellationToken);
        }
    }

    public async Task<IndexedFolder> AddFolderAsync(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

        var existingFolder = await _repository.GetFolderByPathAsync(folderPath);
        if (existingFolder != null)
            return existingFolder;

        var folder = new IndexedFolder
        {
            Path = folderPath,
            LastIndexed = DateTime.MinValue,
            IsExcluded = false
        };

        folder.Id = await _repository.InsertFolderAsync(folder);
        return folder;
    }

    public async Task RemoveFolderAsync(long folderId)
    {
        await _repository.DeleteFolderAsync(folderId);
    }

    public async Task AddExcludedFolderAsync(string folderPath)
    {
        var existingFolder = await _repository.GetFolderByPathAsync(folderPath);
        if (existingFolder != null)
        {
            existingFolder.IsExcluded = true;
            await _repository.UpdateFolderAsync(existingFolder);
            
            // Remove any indexed files from this folder
            await _repository.DeleteFilesByFolderIdAsync(existingFolder.Id);
        }
        else
        {
            var folder = new IndexedFolder
            {
                Path = folderPath,
                LastIndexed = DateTime.MinValue,
                IsExcluded = true
            };
            await _repository.InsertFolderAsync(folder);
        }
    }

    public async Task RemoveExcludedFolderAsync(long folderId)
    {
        await _repository.DeleteFolderAsync(folderId);
    }

    public async Task RebuildIndexAsync(IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        // Clear all data
        await _dbContext.ClearAllDataAsync();
        
        // Get all included folders and re-index
        var folders = await _repository.GetIncludedFoldersAsync();
        
        foreach (var folder in folders)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await IndexFolderAsync(folder.Path, progress, cancellationToken);
        }
    }

    public async Task<IEnumerable<IndexedFolder>> GetIncludedFoldersAsync()
    {
        return await _repository.GetIncludedFoldersAsync();
    }

    public async Task<IEnumerable<IndexedFolder>> GetExcludedFoldersAsync()
    {
        return await _repository.GetExcludedFoldersAsync();
    }

    public async Task<long> GetTotalFileCountAsync()
    {
        return await _repository.GetTotalFileCountAsync();
    }

    public async Task<DateTime?> GetLastIndexTimeAsync()
    {
        var folders = await _repository.GetIncludedFoldersAsync();
        var lastIndexed = folders.MaxBy(f => f.LastIndexed);
        return lastIndexed?.LastIndexed;
    }
}
