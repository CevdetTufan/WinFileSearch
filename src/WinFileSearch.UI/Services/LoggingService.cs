using System.IO;
using Serilog;
using Serilog.Events;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for application-wide logging using Serilog.
/// Provides structured logging with file and debug output sinks.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Initializes the Serilog logger with configured sinks.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Logs a debug-level message. Use for detailed diagnostic information.
    /// </summary>
    /// <param name="message">The message template with optional placeholders.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs an information-level message. Use for general operational events.
    /// </summary>
    /// <param name="message">The message template with optional placeholders.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void LogInfo(string message, params object[] args);

    /// <summary>
    /// Logs a warning-level message. Use for potentially harmful situations.
    /// </summary>
    /// <param name="message">The message template with optional placeholders.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs an error-level message with optional exception details.
    /// </summary>
    /// <param name="message">The message template with optional placeholders.</param>
    /// <param name="exception">Optional exception to include in the log entry.</param>
    /// <param name="args">Values to substitute into the message template.</param>
    void LogError(string message, Exception? exception = null, params object[] args);

    /// <summary>
    /// Logs a performance measurement for an operation.
    /// </summary>
    /// <param name="operation">The name of the operation being measured.</param>
    /// <param name="duration">The duration of the operation.</param>
    void LogPerformance(string operation, TimeSpan duration);

    /// <summary>
    /// Gets the full path to the current log file.
    /// </summary>
    /// <returns>The log file path.</returns>
    string GetLogFilePath();
}

/// <summary>
/// Default implementation of <see cref="ILoggingService"/> using Serilog.
/// Logs are stored in %LocalAppData%\WinFileSearch\logs with daily rotation.
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingService"/> class.
    /// </summary>
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

    /// <inheritdoc />
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
