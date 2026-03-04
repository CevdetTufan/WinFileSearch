namespace WinFileSearch.Data.Models;

/// <summary>
/// Represents an indexed file entry stored in the database.
/// Contains metadata about a file for searching and display purposes.
/// </summary>
public class FileEntry
{
    /// <summary>
    /// Gets or sets the unique identifier of the file entry.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the file including extension.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full absolute path to the file.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension (e.g., ".txt", ".pdf").
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory path containing the file.
    /// </summary>
    public string Directory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the file creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the parent indexed folder.
    /// </summary>
    public long FolderId { get; set; }

    /// <summary>
    /// Gets or sets the file type category for filtering and display.
    /// </summary>
    public FileCategory Category { get; set; }
}

/// <summary>
/// Defines categories for file type classification.
/// Used for filtering search results by file type.
/// </summary>
public enum FileCategory
{
    /// <summary>Uncategorized or unknown file type.</summary>
    Other = 0,

    /// <summary>Document files (PDF, DOC, TXT, etc.).</summary>
    Document = 1,

    /// <summary>Image files (JPG, PNG, GIF, etc.).</summary>
    Image = 2,

    /// <summary>Audio and video files (MP3, MP4, AVI, etc.).</summary>
    Media = 3,

    /// <summary>Compressed archive files (ZIP, RAR, 7Z, etc.).</summary>
    Archive = 4,

    /// <summary>Source code and script files (CS, JS, PY, etc.).</summary>
    Code = 5
}
