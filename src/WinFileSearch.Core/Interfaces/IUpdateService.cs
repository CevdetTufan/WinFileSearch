using WinFileSearch.Core.Models;

namespace WinFileSearch.Core.Interfaces;

/// <summary>
/// Service for checking and downloading application updates.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks if a new version is available on GitHub.
    /// </summary>
    /// <returns>UpdateInfo if update available, null otherwise.</returns>
    Task<UpdateInfo?> CheckForUpdateAsync();

    /// <summary>
    /// Gets the current application version.
    /// </summary>
    Version CurrentVersion { get; }

    /// <summary>
    /// Opens the release page in the default browser.
    /// </summary>
    void OpenReleasePage(string url);

    /// <summary>
    /// Downloads and launches the update installer.
    /// </summary>
    /// <param name="downloadUrl">The URL to download the update from.</param>
    /// <param name="progress">Progress reporter for download progress.</param>
    Task DownloadAndInstallAsync(string downloadUrl, IProgress<int>? progress = null);
}
