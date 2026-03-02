using System.Windows.Controls;
using WinFileSearch.UI.ViewModels;

namespace WinFileSearch.UI.Views;

public partial class SearchPage : Page
{
    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
