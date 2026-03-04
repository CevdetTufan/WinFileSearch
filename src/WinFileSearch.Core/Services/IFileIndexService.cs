using WinFileSearch.Data.Models;

namespace WinFileSearch.Core.Services;

/// <summary>
/// Service for indexing files and managing indexed folders.
/// Provides functionality to scan directories and maintain a searchable file index.
/// </summary>
public interface IFileIndexService
{
    /// <summary>
    /// Starts indexing the specified folder and all its subdirectories.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to index.</param>
    /// <param name="progress">Optional progress reporter for tracking indexing status.</param>
    /// <param name="cancellationToken">Token to cancel the indexing operation.</param>
    /// <returns>A task representing the asynchronous indexing operation.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the folder does not exist.</exception>
    Task IndexFolderAsync(string folderPath, IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes all configured folders in the inclusion list.
    /// </summary>
    /// <param name="progress">Optional progress reporter for tracking indexing status.</param>
    /// <param name="cancellationToken">Token to cancel the indexing operation.</param>
    /// <returns>A task representing the asynchronous indexing operation.</returns>
    Task IndexAllFoldersAsync(IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a folder to the index list for future indexing.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to add.</param>
    /// <returns>The created or existing <see cref="IndexedFolder"/> entity.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the folder does not exist.</exception>
    Task<IndexedFolder> AddFolderAsync(string folderPath);

    /// <summary>
    /// Removes a folder and all its indexed files from the index.
    /// </summary>
    /// <param name="folderId">The unique identifier of the folder to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveFolderAsync(long folderId);

    /// <summary>
    /// Adds a folder to the exclusion list. Excluded folders are skipped during indexing.
    /// </summary>
    /// <param name="folderPath">The full path of the folder to exclude.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddExcludedFolderAsync(string folderPath);

    /// <summary>
    /// Removes a folder from the exclusion list.
    /// </summary>
    /// <param name="folderId">The unique identifier of the excluded folder to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveExcludedFolderAsync(long folderId);

    /// <summary>
    /// Rebuilds the entire index by clearing existing data and re-indexing all folders.
    /// </summary>
    /// <param name="progress">Optional progress reporter for tracking indexing status.</param>
    /// <param name="cancellationToken">Token to cancel the rebuild operation.</param>
    /// <returns>A task representing the asynchronous rebuild operation.</returns>
    Task RebuildIndexAsync(IProgress<IndexingProgress>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all folders that are configured for indexing.
    /// </summary>
    /// <returns>A collection of included <see cref="IndexedFolder"/> entities.</returns>
    Task<IEnumerable<IndexedFolder>> GetIncludedFoldersAsync();

    /// <summary>
    /// Gets all folders that are excluded from indexing.
    /// </summary>
    /// <returns>A collection of excluded <see cref="IndexedFolder"/> entities.</returns>
    Task<IEnumerable<IndexedFolder>> GetExcludedFoldersAsync();

    /// <summary>
    /// Gets the total count of indexed files across all folders.
    /// </summary>
    /// <returns>The total number of indexed files.</returns>
    Task<long> GetTotalFileCountAsync();

    /// <summary>
    /// Gets the timestamp of the last completed indexing operation.
    /// </summary>
    /// <returns>The last index time, or null if no indexing has occurred.</returns>
    Task<DateTime?> GetLastIndexTimeAsync();
}
