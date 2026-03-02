namespace WinFileSearch.Data.Models;

/// <summary>
/// Represents indexing progress information
/// </summary>
public class IndexingProgress
{
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public string CurrentFolder { get; set; } = string.Empty;
    public string CurrentFile { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }
    
    public double PercentComplete => TotalFiles > 0 
        ? (double)ProcessedFiles / TotalFiles * 100 
        : 0;
}
