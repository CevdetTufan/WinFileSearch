namespace WinFileSearch.Data.Models;

/// <summary>
/// Represents a folder that is configured for indexing.
/// Can be either included (indexed) or excluded (skipped during indexing).
/// </summary>
public class IndexedFolder
{
    /// <summary>
    /// Gets or sets the unique identifier of the folder entry.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the full absolute path to the folder.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the last completed indexing operation.
    /// </summary>
    public DateTime LastIndexed { get; set; }

    /// <summary>
    /// Gets or sets the total number of files indexed from this folder.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Gets or sets the total size in bytes of all indexed files.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Gets or sets whether this folder is excluded from indexing.
    /// Excluded folders and their contents are skipped during index operations.
    /// </summary>
    public bool IsExcluded { get; set; }
}
