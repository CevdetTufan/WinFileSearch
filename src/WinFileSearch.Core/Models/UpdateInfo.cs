using System;

namespace WinFileSearch.Core.Models;

/// <summary>
/// Represents information about an application update.
/// </summary>
public class UpdateInfo
{
    /// <summary>
    /// Gets or sets the new version available.
    /// </summary>
    public Version Version { get; set; } = new();

    /// <summary>
    /// Gets or sets the release notes or changelog.
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the download URL for the update.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the release page URL on GitHub.
    /// </summary>
    public string ReleasePageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publish date of the release.
    /// </summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets whether this is a pre-release (beta/alpha).
    /// </summary>
    public bool IsPreRelease { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets the file size in a human-readable format.
    /// </summary>
    public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
