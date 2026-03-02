namespace WinFileSearch.Data.Models;

/// <summary>
/// Search filter options
/// </summary>
public class SearchFilter
{
    public string? Query { get; set; }
    public FileCategory? Category { get; set; }
    public DateTime? ModifiedAfter { get; set; }
    public DateTime? ModifiedBefore { get; set; }
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
    public string? Location { get; set; }
    public int MaxResults { get; set; } = 100;
}
