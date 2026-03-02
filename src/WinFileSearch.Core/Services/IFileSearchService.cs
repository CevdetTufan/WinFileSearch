using WinFileSearch.Data.Models;

namespace WinFileSearch.Core.Services;

public interface IFileSearchService
{
    /// <summary>
    /// Searches for files matching the query
    /// </summary>
    Task<IEnumerable<FileEntry>> SearchAsync(string query, FileCategory? category = null, int maxResults = 100);
    
    /// <summary>
    /// Searches with advanced filters
    /// </summary>
    Task<IEnumerable<FileEntry>> SearchAsync(SearchFilter filter);
    
    /// <summary>
    /// Gets recently indexed/modified files
    /// </summary>
    Task<IEnumerable<FileEntry>> GetRecentFilesAsync(int count = 20);
    
    /// <summary>
    /// Opens a file with the default application
    /// </summary>
    void OpenFile(string fullPath);
    
    /// <summary>
    /// Opens the folder containing the file
    /// </summary>
    void OpenFileLocation(string fullPath);
}
