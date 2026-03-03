using System.IO;
using Serilog;
using Serilog.Events;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for application-wide logging using Serilog
/// </summary>
public interface ILoggingService
{
    void Initialize();
    void LogDebug(string message, params object[] args);
    void LogInfo(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, Exception? exception = null, params object[] args);
    void LogPerformance(string operation, TimeSpan duration);
    string GetLogFilePath();
}

public class LoggingService : ILoggingService
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private bool _isInitialized;

    public LoggingService()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinFileSearch",
            "logs");

        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }

        _logFilePath = Path.Combine(_logDirectory, "winfilesearch-.log");
    }

    public void Initialize()
    {
        if (_isInitialized) return;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                _logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                shared: true)
            .CreateLogger();

        _isInitialized = true;
        LogInfo("WinFileSearch logging initialized");
        LogInfo("Log directory: {LogDirectory}", _logDirectory);
    }

    public void LogDebug(string message, params object[] args)
    {
        Log.Debug(message, args);
    }

    public void LogInfo(string message, params object[] args)
    {
        Log.Information(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        Log.Warning(message, args);
    }

    public void LogError(string message, Exception? exception = null, params object[] args)
    {
        if (exception != null)
            Log.Error(exception, message, args);
        else
            Log.Error(message, args);
    }

    public void LogPerformance(string operation, TimeSpan duration)
    {
        Log.Information("PERF: {Operation} completed in {Duration:N2}ms", operation, duration.TotalMilliseconds);
    }

    public string GetLogFilePath() => _logDirectory;

    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }
}
