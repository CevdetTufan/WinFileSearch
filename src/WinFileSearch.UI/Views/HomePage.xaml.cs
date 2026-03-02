using System.Windows.Controls;
using WinFileSearch.UI.ViewModels;

namespace WinFileSearch.UI.Views;

public partial class HomePage : Page
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
