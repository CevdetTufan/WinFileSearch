namespace WinFileSearch.Data.Models;

/// <summary>
/// Represents an indexed file in the database
/// </summary>
public class FileEntry
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public long FolderId { get; set; }
    
    /// <summary>
    /// File type category for filtering
    /// </summary>
    public FileCategory Category { get; set; }
}

public enum FileCategory
{
    Other = 0,
    Document = 1,
    Image = 2,
    Media = 3,
    Archive = 4,
    Code = 5
}
