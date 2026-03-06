using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinFileSearch.UI.ViewModels;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;

namespace WinFileSearch.UI.Views;

public partial class SettingsPage : Page
{
    private const string SecondaryBackgroundBrushKey = "SecondaryBackgroundBrush";
    private readonly SettingsViewModel _viewModel;
    private Border? _dropZone;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Get reference to DropZone after initialization
        Loaded += (s, e) => _dropZone = FindName("DropZone") as Border;
    }

    private void DropZone_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            // Check if any dropped item is a directory
            if (files != null && files.Any(Directory.Exists))
            {
                e.Effects = DragDropEffects.Copy;
                // Visual feedback - highlight drop zone
                if (_dropZone != null)
                {
                    _dropZone.BorderBrush = (Brush)Application.Current.Resources["AccentBrush"];
                    _dropZone.Background = new SolidColorBrush(Color.FromArgb(30, 0, 122, 255));
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        // Reset visual feedback
        if (_dropZone != null)
        {
            _dropZone.BorderBrush = (Brush)Application.Current.Resources[SecondaryBackgroundBrushKey];
            _dropZone.Background = (Brush)Application.Current.Resources[SecondaryBackgroundBrushKey];
        }
        e.Handled = true;
    }

    private void DropZone_Drop(object sender, DragEventArgs e)
    {
        // Reset visual feedback
        if (_dropZone != null)
        {
            _dropZone.BorderBrush = (Brush)Application.Current.Resources[SecondaryBackgroundBrushKey];
            _dropZone.Background = (Brush)Application.Current.Resources[SecondaryBackgroundBrushKey];
        }

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var paths = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (paths != null)
            {
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        // Add folder via ViewModel command
                        _viewModel.AddFolderByPath(path);
                    }
                }
            }
        }
        e.Handled = true;
    }
}
