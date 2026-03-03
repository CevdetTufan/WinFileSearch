using System.Windows;
using System.Windows.Controls;
using WinFileSearch.UI.ViewModels;

namespace WinFileSearch.UI.Views;

public partial class SearchPage : Page
{
    private readonly SearchViewModel _viewModel;

    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // Show history when search box is focused and empty
        if (string.IsNullOrEmpty(_viewModel.SearchQuery) && _viewModel.SearchHistory.Count > 0)
        {
            _viewModel.ShowHistory = true;
        }
    }
}
