using WinFileSearch.Data.Models;

namespace WinFileSearch.Core.Services;

public interface IFileIndexService
{
    /// <summary>
    /// Starts indexing the specified folder
    /// </summary>
    Task IndexFolderAsync(string folderPath, IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Indexes all configured folders
    /// </summary>
    Task IndexAllFoldersAsync(IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a folder to the index list
    /// </summary>
    Task<IndexedFolder> AddFolderAsync(string folderPath);
    
    /// <summary>
    /// Removes a folder from the index
    /// </summary>
    Task RemoveFolderAsync(long folderId);
    
    /// <summary>
    /// Adds a folder to the exclusion list
    /// </summary>
    Task AddExcludedFolderAsync(string folderPath);
    
    /// <summary>
    /// Removes a folder from exclusion list
    /// </summary>
    Task RemoveExcludedFolderAsync(long folderId);
    
    /// <summary>
    /// Rebuilds the entire index
    /// </summary>
    Task RebuildIndexAsync(IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets included folders
    /// </summary>
    Task<IEnumerable<IndexedFolder>> GetIncludedFoldersAsync();
    
    /// <summary>
    /// Gets excluded folders
    /// </summary>
    Task<IEnumerable<IndexedFolder>> GetExcludedFoldersAsync();
    
    /// <summary>
    /// Gets total indexed file count
    /// </summary>
    Task<long> GetTotalFileCountAsync();
    
    /// <summary>
    /// Gets the last indexing time
    /// </summary>
    Task<DateTime?> GetLastIndexTimeAsync();
}
