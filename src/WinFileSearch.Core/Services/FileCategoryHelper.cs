using WinFileSearch.Data.Models;

namespace WinFileSearch.Core.Services;

/// <summary>
/// Helper class to determine file category based on extension
/// </summary>
public static class FileCategoryHelper
{
    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".rtf", ".odt", ".ods", ".odp", ".csv", ".md"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp",
        ".ico", ".tiff", ".tif", ".raw", ".psd", ".ai"
    };

    private static readonly HashSet<string> MediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv",
        ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".webm"
    };

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz"
    };

    private static readonly HashSet<string> CodeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h",
        ".html", ".css", ".json", ".xml", ".yaml", ".yml", ".sql"
    };

    public static FileCategory GetCategory(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return FileCategory.Other;

        if (DocumentExtensions.Contains(extension))
            return FileCategory.Document;
        
        if (ImageExtensions.Contains(extension))
            return FileCategory.Image;
        
        if (MediaExtensions.Contains(extension))
            return FileCategory.Media;
        
        if (ArchiveExtensions.Contains(extension))
            return FileCategory.Archive;
        
        if (CodeExtensions.Contains(extension))
            return FileCategory.Code;

        return FileCategory.Other;
    }
}
