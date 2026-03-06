using WinFileSearch.Data;
using WinFileSearch.Data.Models;
using WinFileSearch.Data.Repositories;

namespace WinFileSearch.Core.Services;

/// <summary>
/// High-performance file indexing service with batch processing and optimized I/O.
/// </summary>
public class FileIndexService(IFileRepository repository, FileSearchDbContext dbContext) : IFileIndexService
{
    private readonly IFileRepository _repository = repository;
    private readonly FileSearchDbContext _dbContext = dbContext;

	// Optimized constants for better throughput
	private const int BatchSize = 1000;              // Increased from 500 for fewer DB transactions
	private const int ProgressReportInterval = 200;   // Report less frequently for better performance
	private const int UiYieldInterval = 2000;         // Yield to UI every N files

	private static bool IsExcludedPath(string filePath, List<IndexedFolder> excludedFolders)
	{
		var directory = Path.GetDirectoryName(filePath) ?? "";
		return excludedFolders.Any(ef => directory.StartsWith(ef.Path, StringComparison.OrdinalIgnoreCase));
	}

	private static FileEntry? TryCreateFileEntry(string filePath, long folderId)
	{
		try
		{
			var fileInfo = new FileInfo(filePath);
			var extension = fileInfo.Extension.ToLowerInvariant();

			return new FileEntry
			{
				FileName = fileInfo.Name,
				FullPath = fileInfo.FullName,
				Extension = extension,
				Directory = fileInfo.DirectoryName ?? "",
				Size = fileInfo.Length,
				CreatedAt = fileInfo.CreationTime,
				ModifiedAt = fileInfo.LastWriteTime,
				FolderId = folderId,
				Category = FileCategoryHelper.GetCategory(extension)
			};
		}
		catch
		{
			return null;
		}
	}

	private async Task ProcessBatchAsync(
		List<FileEntry> batch,
		IProgress<IndexingProgress>? progress,
		IndexingProgress progressInfo,
		int progressCounter,
		CancellationToken cancellationToken)
	{
		if (batch.Count >= BatchSize)
		{
			await _repository.InsertFilesAsync(batch);
			batch.Clear();
			progress?.Report(progressInfo);
			await Task.Delay(1, cancellationToken);
		}
		else if (progressCounter % ProgressReportInterval == 0)
		{
			progress?.Report(progressInfo);
			if (progressCounter % UiYieldInterval == 0)
			{
				await Task.Yield();
			}
		}
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

        // Use streaming enumeration to avoid loading all files into memory
        var fileEnumerable = Directory.EnumerateFiles(folderPath, "*", new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true
        });

        // Estimate total (quick count using parallel enumeration with timeout)
        progressInfo.TotalFiles = await Task.Run(() => 
        {
            try
            {
                return fileEnumerable.Take(100000).Count(); // Limit to prevent long delays
            }
            catch
            {
                return 0;
            }
        }, cancellationToken);

        progress?.Report(progressInfo);

        var batch = new List<FileEntry>();
        long totalSize = 0;
        int fileCount = 0;
        int progressCounter = 0;

        // Re-enumerate for actual processing
        foreach (var filePath in Directory.EnumerateFiles(folderPath, "*", new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true
        }))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                progressInfo.IsCancelled = true;
                progress?.Report(progressInfo);
                return;
            }

            if (IsExcludedPath(filePath, excludedFolders))
                continue;

            var fileEntry = TryCreateFileEntry(filePath, folder.Id);
            if (fileEntry == null)
                continue;

            batch.Add(fileEntry);
            totalSize += fileEntry.Size;
            fileCount++;
            progressCounter++;

            progressInfo.ProcessedFiles++;
            progressInfo.CurrentFile = fileEntry.FileName;

            await ProcessBatchAsync(batch, progress, progressInfo, progressCounter, cancellationToken);
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
        // Get all included folders BEFORE clearing
        var folders = (await _repository.GetIncludedFoldersAsync()).ToList();

        if (folders.Count == 0)
        {
            progress?.Report(new IndexingProgress { IsCompleted = true });
            return;
        }

        // Clear files only, preserve folder list
        await _dbContext.ClearFilesOnlyAsync();

        // Re-index all folders - filter to existing directories and project to paths
        var folderPaths = folders
            .Select(folder => folder.Path)
            .Where(Directory.Exists);

        foreach (var path in folderPaths)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await IndexFolderAsync(path, progress, cancellationToken);
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
		return folders.Max(f => (DateTime?)f.LastIndexed);
	}
}
