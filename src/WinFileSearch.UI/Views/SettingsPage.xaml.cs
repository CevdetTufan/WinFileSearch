using System.Windows.Controls;
using WinFileSearch.UI.ViewModels;

namespace WinFileSearch.UI.Views;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
