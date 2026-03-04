using WinFileSearch.Data.Models;

namespace WinFileSearch.Core.Services;

/// <summary>
/// Service for searching indexed files using full-text search capabilities.
/// Provides fast file lookup using SQLite FTS5 technology.
/// </summary>
public interface IFileSearchService
{
    /// <summary>
    /// Searches for files matching the specified query string.
    /// </summary>
    /// <param name="query">The search query text. Supports partial matching.</param>
    /// <param name="category">Optional file category filter (Document, Image, Media, etc.).</param>
    /// <param name="maxResults">Maximum number of results to return. Default is 100.</param>
    /// <returns>A collection of <see cref="FileEntry"/> objects matching the search criteria.</returns>
    Task<IEnumerable<FileEntry>> SearchAsync(string query, FileCategory? category = null, int maxResults = 100);

    /// <summary>
    /// Searches for files using advanced filter options.
    /// </summary>
    /// <param name="filter">A <see cref="SearchFilter"/> object containing search criteria.</param>
    /// <returns>A collection of <see cref="FileEntry"/> objects matching the filter criteria.</returns>
    Task<IEnumerable<FileEntry>> SearchAsync(SearchFilter filter);

    /// <summary>
    /// Gets recently indexed or modified files.
    /// </summary>
    /// <param name="count">The number of recent files to retrieve. Default is 20.</param>
    /// <returns>A collection of recently modified <see cref="FileEntry"/> objects.</returns>
    Task<IEnumerable<FileEntry>> GetRecentFilesAsync(int count = 20);

    /// <summary>
    /// Opens a file using the system's default application.
    /// </summary>
    /// <param name="fullPath">The full path to the file to open.</param>
    void OpenFile(string fullPath);

    /// <summary>
    /// Opens Windows Explorer and selects the specified file.
    /// </summary>
    /// <param name="fullPath">The full path to the file to reveal.</param>
    void OpenFileLocation(string fullPath);
}
