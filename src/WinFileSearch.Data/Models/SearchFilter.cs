namespace WinFileSearch.Data.Models;

/// <summary>
/// Represents search filter options for advanced file searching.
/// All filter properties are optional and can be combined for refined results.
/// </summary>
public class SearchFilter
{
    /// <summary>
    /// Gets or sets the search query text. Supports partial matching via FTS5.
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Gets or sets the file category filter. When set, only files of this category are returned.
    /// </summary>
    public FileCategory? Category { get; set; }

    /// <summary>
    /// Gets or sets the minimum modification date. Files modified before this date are excluded.
    /// </summary>
    public DateTime? ModifiedAfter { get; set; }

    /// <summary>
    /// Gets or sets the maximum modification date. Files modified after this date are excluded.
    /// </summary>
    public DateTime? ModifiedBefore { get; set; }

    /// <summary>
    /// Gets or sets the minimum file size in bytes. Smaller files are excluded.
    /// </summary>
    public long? MinSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum file size in bytes. Larger files are excluded.
    /// </summary>
    public long? MaxSize { get; set; }

    /// <summary>
    /// Gets or sets the directory path filter. Only files in this location are included.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results to return. Default is 100.
    /// </summary>
    public int MaxResults { get; set; } = 100;
}
