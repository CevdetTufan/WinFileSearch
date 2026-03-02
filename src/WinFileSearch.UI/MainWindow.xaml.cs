using System.Windows;
using WinFileSearch.UI.Views;

namespace WinFileSearch.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly HomePage _homePage;
    private readonly SearchPage _searchPage;
    private readonly SettingsPage _settingsPage;

    public MainWindow(HomePage homePage, SearchPage searchPage, SettingsPage settingsPage)
    {
        InitializeComponent();

        _homePage = homePage;
        _searchPage = searchPage;
        _settingsPage = settingsPage;

        // Navigate to home page on startup
        MainFrame.Navigate(_homePage);
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