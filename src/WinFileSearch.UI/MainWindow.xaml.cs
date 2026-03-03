using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WinFileSearch.UI.Services;
using WinFileSearch.UI.ViewModels;
using WinFileSearch.UI.Views;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WinFileSearch.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly HomePage _homePage;
    private readonly SearchPage _searchPage;
    private readonly SettingsPage _settingsPage;
    private readonly SearchViewModel _searchViewModel;
    private readonly INavigationService _navigationService;

    // Commands for keyboard shortcuts
    public ICommand NavigateToHomeCommand { get; }
    public ICommand NavigateToSearchCommand { get; }
    public ICommand NavigateToRecentCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }

    public MainWindow(
        HomePage homePage, 
        SearchPage searchPage, 
        SettingsPage settingsPage,
        INavigationService navigationService,
        SearchViewModel searchViewModel)
    {
        InitializeComponent();

        _homePage = homePage;
        _searchPage = searchPage;
        _settingsPage = settingsPage;
        _searchViewModel = searchViewModel;
        _navigationService = navigationService;

        // Initialize commands
        NavigateToHomeCommand = new RelayCommand(() => { HomeNav.IsChecked = true; });
        NavigateToSearchCommand = new RelayCommand(() => { SearchNav.IsChecked = true; });
        NavigateToRecentCommand = new RelayCommand(() => { RecentNav.IsChecked = true; });
        NavigateToSettingsCommand = new RelayCommand(() => { SettingsNav.IsChecked = true; });

        DataContext = this;

        // Subscribe to navigation events
        navigationService.NavigationRequested += OnNavigationRequested;

        // Navigate to home page on startup
        MainFrame.Navigate(_homePage);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Escape key handling
        if (e.Key == Key.Escape)
        {
            // If on search page, clear search or close preview
            if (MainFrame.Content == _searchPage)
            {
                if (_searchViewModel.ShowPreview)
                {
                    _searchViewModel.ClosePreviewCommand.Execute(null);
                }
                else if (!string.IsNullOrEmpty(_searchViewModel.SearchQuery))
                {
                    _searchViewModel.SearchQuery = string.Empty;
                }
            }
            e.Handled = true;
        }
    }

    private void OnNavigationRequested(object? sender, NavigationEventArgs e)
    {
        switch (e.PageName)
        {
            case "Search":
                SearchNav.IsChecked = true;
                MainFrame.Navigate(_searchPage);
                // If search query was passed, set it
                if (e.Parameter is string searchQuery && !string.IsNullOrEmpty(searchQuery))
                {
                    _searchViewModel.SearchQuery = searchQuery;
                }
                break;
            case "Home":
                HomeNav.IsChecked = true;
                MainFrame.Navigate(_homePage);
                break;
            case "Settings":
                SettingsNav.IsChecked = true;
                MainFrame.Navigate(_settingsPage);
                break;
        }
    }

    private void HomeNav_Checked(object sender, RoutedEventArgs e)
    {
        MainFrame?.Navigate(_homePage);
    }

    private void SearchNav_Checked(object sender, RoutedEventArgs e)
    {
        MainFrame?.Navigate(_searchPage);
    }

    private void RecentNav_Checked(object sender, RoutedEventArgs e)
    {
        // Recent is part of home page for now
        MainFrame?.Navigate(_homePage);
    }

    private void SettingsNav_Checked(object sender, RoutedEventArgs e)
    {
        MainFrame?.Navigate(_settingsPage);
    }
}