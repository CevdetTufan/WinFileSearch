using System.Diagnostics;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Performance metrics data model
/// </summary>
public class PerformanceMetrics
{
    public long TotalSearches { get; set; }
    public double AverageSearchTimeMs { get; set; }
    public double LastSearchTimeMs { get; set; }
    public long TotalFilesIndexed { get; set; }
    public double IndexingTimeMs { get; set; }
    public long MemoryUsageMB { get; set; }
    public double CpuUsagePercent { get; set; }
    public DateTime LastUpdated { get; set; }
    public TimeSpan Uptime { get; set; }
}

/// <summary>
/// Service for tracking application performance metrics
/// </summary>
public interface IPerformanceMetricsService
{
    PerformanceMetrics GetMetrics();
    void RecordSearch(TimeSpan duration);
    void RecordIndexing(TimeSpan duration, long filesIndexed);
    void Reset();
}

public class PerformanceMetricsService : IPerformanceMetricsService
{
    private readonly DateTime _startTime;
    private readonly List<double> _searchTimes = [];
    private readonly object _lock = new();
    private long _totalFilesIndexed;
    private double _lastIndexingTimeMs;

    public PerformanceMetricsService()
    {
        _startTime = DateTime.UtcNow;
    }

    public PerformanceMetrics GetMetrics()
    {
        lock (_lock)
        {
            var process = Process.GetCurrentProcess();
            
            return new PerformanceMetrics
            {
                TotalSearches = _searchTimes.Count,
                AverageSearchTimeMs = _searchTimes.Count > 0 ? _searchTimes.Average() : 0,
                LastSearchTimeMs = _searchTimes.Count > 0 ? _searchTimes.Last() : 0,
                TotalFilesIndexed = _totalFilesIndexed,
                IndexingTimeMs = _lastIndexingTimeMs,
                MemoryUsageMB = process.WorkingSet64 / (1024 * 1024),
                CpuUsagePercent = 0, // CPU usage requires more complex tracking
                LastUpdated = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - _startTime
            };
        }
    }

    public void RecordSearch(TimeSpan duration)
    {
        lock (_lock)
        {
            _searchTimes.Add(duration.TotalMilliseconds);
            
            // Keep only last 100 searches for average calculation
            if (_searchTimes.Count > 100)
            {
                _searchTimes.RemoveAt(0);
            }
        }
    }

    public void RecordIndexing(TimeSpan duration, long filesIndexed)
    {
        lock (_lock)
        {
            _lastIndexingTimeMs = duration.TotalMilliseconds;
            _totalFilesIndexed = filesIndexed;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _searchTimes.Clear();
            _totalFilesIndexed = 0;
            _lastIndexingTimeMs = 0;
        }
    }
}
