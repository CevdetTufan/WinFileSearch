using Microsoft.Win32;
using System.Diagnostics;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for managing Windows startup options
/// </summary>
public interface IStartupService
{
    bool IsStartupEnabled { get; }
    void EnableStartup();
    void DisableStartup();
    void SetStartup(bool enable);
}

public class StartupService : IStartupService
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "WinFileSearch";

    public bool IsStartupEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }
    }

    public void EnableStartup()
    {
        try
        {
            var exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath))
                return;

            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            key?.SetValue(AppName, $"\"{exePath}\" --minimized");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to enable startup: {ex.Message}");
        }
    }

    public void DisableStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            key?.DeleteValue(AppName, false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to disable startup: {ex.Message}");
        }
    }

    public void SetStartup(bool enable)
    {
        if (enable)
            EnableStartup();
        else
            DisableStartup();
    }

    private static string? GetExecutablePath()
    {
        var process = Process.GetCurrentProcess();
        var module = process.MainModule;
        return module?.FileName;
    }
}
