using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WinFileSearch.Core.Interfaces;
using WinFileSearch.Core.Services;
using WinFileSearch.Data;
using WinFileSearch.Data.Repositories;
using WinFileSearch.UI.Services;
using WinFileSearch.UI.ViewModels;
using WinFileSearch.UI.Views;

namespace WinFileSearch.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IServiceProvider _serviceProvider;
    private FileWatcherSyncService? _watcherSyncService;
    private ISystemTrayService? _systemTrayService;
    private IGlobalHotkeyService? _globalHotkeyService;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Data Layer
        services.AddSingleton<FileSearchDbContext>();
        services.AddSingleton<IFileRepository, FileRepository>();

        // Core Services
        services.AddSingleton<IFileIndexService, FileIndexService>();
        services.AddSingleton<IFileSearchService, FileSearchService>();
        services.AddSingleton<IFileWatcherService, FileWatcherService>();
        services.AddSingleton<IUpdateService, UpdateService>();

        // UI Services
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IPerformanceMetricsService, PerformanceMetricsService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISearchHistoryService, SearchHistoryService>();
        services.AddSingleton<IFavoritesService, FavoritesService>();
        services.AddSingleton<ISystemTrayService, SystemTrayService>();
        services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();
        services.AddSingleton<IStartupService, StartupService>();

        // ViewModels
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SearchViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<HomePage>();
        services.AddTransient<SearchPage>();
        services.AddTransient<SettingsPage>();
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize logging first
        var loggingService = _serviceProvider.GetRequiredService<ILoggingService>();
        loggingService.Initialize();
        loggingService.LogInfo("Application starting...");

        // Initialize database
        var dbContext = _serviceProvider.GetRequiredService<FileSearchDbContext>();
        await dbContext.InitializeDatabaseAsync();
        loggingService.LogInfo("Database initialized");

        // Start file watcher
        var watcherService = _serviceProvider.GetRequiredService<IFileWatcherService>();
        await watcherService.StartWatchingAsync();
        loggingService.LogInfo("File watcher started");

        // Initialize watcher sync service (connects FileWatcher to DB)
        var repository = _serviceProvider.GetRequiredService<IFileRepository>();
        _watcherSyncService = new FileWatcherSyncService(watcherService, repository);

        // Show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        loggingService.LogInfo("Main window displayed");

        // Initialize system tray
        _systemTrayService = _serviceProvider.GetRequiredService<ISystemTrayService>();
        _systemTrayService.Initialize(mainWindow);

        // Initialize global hotkey (Win+Shift+F)
        _globalHotkeyService = _serviceProvider.GetRequiredService<IGlobalHotkeyService>();
        _globalHotkeyService.Initialize(mainWindow);
        _globalHotkeyService.HotkeyPressed += (s, e) =>
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
            mainWindow.Focus();

            // Navigate to search and focus search box
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            navigationService.NavigateToSearch();
        };

        // Check for updates in background
        _ = CheckForUpdatesAsync(loggingService);
    }

    private async Task CheckForUpdatesAsync(ILoggingService loggingService)
    {
        try
        {
            await Task.Delay(5000); // Wait 5 seconds after startup

            var updateService = _serviceProvider.GetRequiredService<IUpdateService>();
            var updateInfo = await updateService.CheckForUpdateAsync();

            if (updateInfo != null)
            {
                loggingService.LogInfo($"Update available: v{updateInfo.Version}");

                // Store update info for SettingsPage to display
                System.Windows.Application.Current.Properties["UpdateAvailable"] = updateInfo;
            }
        }
        catch (Exception ex)
        {
            loggingService.LogError("Failed to check for updates", ex);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var loggingService = _serviceProvider.GetRequiredService<ILoggingService>();
        loggingService.LogInfo("Application shutting down...");

        // Dispose global hotkey
        _globalHotkeyService?.Dispose();

        // Dispose system tray
        _systemTrayService?.Dispose();

        // Dispose sync service
        _watcherSyncService?.Dispose();

        // Stop file watcher
        var watcherService = _serviceProvider.GetRequiredService<IFileWatcherService>();
        watcherService.StopWatching();

        // Dispose database context
        var dbContext = _serviceProvider.GetRequiredService<FileSearchDbContext>();
        dbContext.Dispose();

        loggingService.LogInfo("Application shutdown complete");
        LoggingService.CloseAndFlush();

        base.OnExit(e);
    }
}

