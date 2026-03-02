using WinFileSearch.Data.Models;

namespace WinFileSearch.Data.Repositories;

public interface IFileRepository
{
    // File operations
    Task<long> InsertFileAsync(FileEntry file);
    Task InsertFilesAsync(IEnumerable<FileEntry> files);
    Task<FileEntry?> GetFileByPathAsync(string fullPath);
    Task<IEnumerable<FileEntry>> SearchFilesAsync(SearchFilter filter);
    Task<IEnumerable<FileEntry>> GetRecentFilesAsync(int count = 20);
    Task DeleteFileAsync(long id);
    Task DeleteFileByPathAsync(string fullPath);
    Task UpdateFileAsync(FileEntry file);
    Task<long> GetTotalFileCountAsync();
    
    // Folder operations
    Task<long> InsertFolderAsync(IndexedFolder folder);
    Task<IndexedFolder?> GetFolderByPathAsync(string path);
    Task<IEnumerable<IndexedFolder>> GetAllFoldersAsync(bool includeExcluded = false);
    Task<IEnumerable<IndexedFolder>> GetIncludedFoldersAsync();
    Task<IEnumerable<IndexedFolder>> GetExcludedFoldersAsync();
    Task UpdateFolderAsync(IndexedFolder folder);
    Task DeleteFolderAsync(long id);
    Task DeleteFilesByFolderIdAsync(long folderId);
}
