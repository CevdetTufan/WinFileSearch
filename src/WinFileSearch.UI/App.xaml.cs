using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WinFileSearch.Core.Services;
using WinFileSearch.Data;
using WinFileSearch.Data.Repositories;
using WinFileSearch.UI.ViewModels;
using WinFileSearch.UI.Views;

namespace WinFileSearch.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IServiceProvider _serviceProvider;

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

        // ViewModels
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<HomePage>();
        services.AddTransient<SearchPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize database
        var dbContext = _serviceProvider.GetRequiredService<FileSearchDbContext>();
        await dbContext.InitializeDatabaseAsync();

        // Start file watcher
        var watcherService = _serviceProvider.GetRequiredService<IFileWatcherService>();
        await watcherService.StartWatchingAsync();

        // Show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Stop file watcher
        var watcherService = _serviceProvider.GetRequiredService<IFileWatcherService>();
        watcherService.StopWatching();

        // Dispose database context
        var dbContext = _serviceProvider.GetRequiredService<FileSearchDbContext>();
        dbContext.Dispose();

        base.OnExit(e);
    }
}

