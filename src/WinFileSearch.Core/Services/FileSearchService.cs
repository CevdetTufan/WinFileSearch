using System.Diagnostics;
using WinFileSearch.Data.Models;
using WinFileSearch.Data.Repositories;

namespace WinFileSearch.Core.Services;

public class FileSearchService(IFileRepository repository) : IFileSearchService
{
    private readonly IFileRepository _repository = repository;

	public async Task<IEnumerable<FileEntry>> SearchAsync(string query, FileCategory? category = null, int maxResults = 100)
    {
        var filter = new SearchFilter
        {
            Query = query,
            Category = category,
            MaxResults = maxResults
        };

        return await SearchAsync(filter);
    }

    public async Task<IEnumerable<FileEntry>> SearchAsync(SearchFilter filter)
    {
        return await _repository.SearchFilesAsync(filter);
    }

    public async Task<IEnumerable<FileEntry>> GetRecentFilesAsync(int count = 20)
    {
        return await _repository.GetRecentFilesAsync(count);
    }

    public void OpenFile(string fullPath)
    {
        if (!File.Exists(fullPath))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            // Handle silently
        }
    }

    public void OpenFileLocation(string fullPath)
    {
        if (!File.Exists(fullPath))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{fullPath}\"",
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            // Handle silently
        }
    }
}
