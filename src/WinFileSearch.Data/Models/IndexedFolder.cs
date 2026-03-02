namespace WinFileSearch.Data.Models;

/// <summary>
/// Represents a folder that is being indexed
/// </summary>
public class IndexedFolder
{
    public long Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTime LastIndexed { get; set; }
    public int FileCount { get; set; }
    public long TotalSize { get; set; }
    public bool IsExcluded { get; set; }
}
