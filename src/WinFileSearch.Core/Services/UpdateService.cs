using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WinFileSearch.Core.Interfaces;
using WinFileSearch.Core.Models;

namespace WinFileSearch.Core.Services;

/// <summary>
/// Service for checking and downloading application updates from GitHub.
/// </summary>
public class UpdateService : IUpdateService
{
    private const string GitHubOwner = "CevdetTufan";
    private const string GitHubRepo = "WinFileSearch";
    private const string GitHubApiUrl = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";

    private readonly HttpClient _httpClient;

    /// <inheritdoc/>
    public Version CurrentVersion { get; }

    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "WinFileSearch-UpdateChecker");

        // Get current version from assembly
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        CurrentVersion = assembly.GetName().Version ?? new Version(1, 0, 0);
    }

    /// <inheritdoc/>
    public async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(GitHubApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var release = await response.Content.ReadFromJsonAsync<GitHubRelease>(options);

            if (release == null)
            {
                return null;
            }

            // Parse version from tag (remove 'v' prefix and any suffix like -beta.1)
            var versionString = release.TagName.TrimStart('v');
            var dashIndex = versionString.IndexOf('-');
            if (dashIndex > 0)
            {
                versionString = versionString.Substring(0, dashIndex);
            }

            if (!Version.TryParse(versionString, out var latestVersion))
            {
                return null;
            }

            // Compare versions
            if (latestVersion <= CurrentVersion)
            {
                return null;
            }

            // Find the self-contained download, fallback to any zip file
            var asset = release.Assets.FirstOrDefault(a =>
                a.Name.Contains("selfcontained", StringComparison.OrdinalIgnoreCase) &&
                a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                ?? release.Assets.FirstOrDefault(a =>
                    a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

            var downloadUrl = asset?.BrowserDownloadUrl ?? string.Empty;
            var fileSize = asset?.Size ?? 0;

            return new UpdateInfo
            {
                Version = latestVersion,
                ReleaseNotes = release.Body ?? string.Empty,
                DownloadUrl = downloadUrl,
                ReleasePageUrl = release.HtmlUrl,
                PublishedAt = release.PublishedAt,
                IsPreRelease = release.Prerelease,
                FileSizeBytes = fileSize
            };
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public void OpenReleasePage(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Silently fail if cannot open browser
        }
    }

    /// <inheritdoc/>
    public async Task DownloadAndInstallAsync(string downloadUrl, IProgress<int>? progress = null)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "WinFileSearch_Update");
        Directory.CreateDirectory(tempPath);

        var zipPath = Path.Combine(tempPath, "update.zip");
        var extractPath = Path.Combine(tempPath, "extracted");

        // Download the file with progress
        using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var downloadedBytes = 0L;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                downloadedBytes += bytesRead;

                if (totalBytes > 0)
                {
                    var percentComplete = (int)((downloadedBytes * 100) / totalBytes);
                    progress?.Report(percentComplete);
                }
            }
        }

        // Extract the zip
        if (Directory.Exists(extractPath))
            Directory.Delete(extractPath, true);

        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

        // Create update script
        var currentExePath = Environment.ProcessPath ?? Assembly.GetEntryAssembly()?.Location ?? "";
        var currentDir = Path.GetDirectoryName(currentExePath) ?? "";
        var scriptPath = Path.Combine(tempPath, "update.bat");

        var script = $"""
            @echo off
            echo Updating WinFileSearch...
            timeout /t 2 /nobreak > nul
            xcopy /s /y "{extractPath}\*" "{currentDir}\"
            start "" "{currentExePath}"
            del "%~f0"
            """;

        await File.WriteAllTextAsync(scriptPath, script);

        // Start the update script and exit
        Process.Start(new ProcessStartInfo
        {
            FileName = scriptPath,
            UseShellExecute = true,
            CreateNoWindow = true
        });

        // Exit the application
        Environment.Exit(0);
    }

    // GitHub API response models
    private class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonPropertyName("assets")]
        public GitHubAsset[] Assets { get; set; } = [];
    }

    private class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}
