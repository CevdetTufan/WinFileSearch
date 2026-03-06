using System.Windows;
using Application = System.Windows.Application;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for managing system tray icon and functionality
/// </summary>
public interface ISystemTrayService : IDisposable
{
    void Initialize(Window mainWindow);
    void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info);
}

public class SystemTrayService : ISystemTrayService
{
    private NotifyIcon? _notifyIcon;
    private Window? _mainWindow;
    private bool _isExiting;
    private bool _disposed;

    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;

        // Create notification icon
        _notifyIcon = new NotifyIcon
        {
            Icon = CreateAppIcon(),
            Text = "WinFileSearch - Quick File Search",
            Visible = true
        };

        // Create context menu
        var contextMenu = new ContextMenuStrip();
        
        var showItem = new ToolStripMenuItem("Show Window");
        showItem.Click += (s, e) => ShowMainWindow();
        showItem.Font = new Font(showItem.Font, System.Drawing.FontStyle.Bold);
        
        var searchItem = new ToolStripMenuItem("Quick Search (Win+Shift+F)");
        searchItem.Click += (s, e) => ShowMainWindow();
        
        var separatorItem = new ToolStripSeparator();
        
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => ExitApplication();

        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(searchItem);
        contextMenu.Items.Add(separatorItem);
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;

        // Double-click to show window
        _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

        // Handle window state changes
        _mainWindow.StateChanged += OnWindowStateChanged;
        _mainWindow.Closing += OnWindowClosing;
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (_mainWindow?.WindowState == WindowState.Minimized)
        {
            _mainWindow.Hide();
            ShowBalloonTip("WinFileSearch", "Application minimized to system tray. Double-click to restore.");
        }
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExiting && _mainWindow != null)
        {
            e.Cancel = true;
            _mainWindow.WindowState = WindowState.Minimized;
            _mainWindow.Hide();
            ShowBalloonTip("WinFileSearch", "Application is still running in the system tray.");
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _mainWindow.Focus();
        }
    }

    private void ExitApplication()
    {
        _isExiting = true;
        Application.Current?.Shutdown();
    }

    public void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon?.ShowBalloonTip(2000, title, message, icon);
    }

    private static Icon CreateAppIcon()
    {
        // Create a simple app icon programmatically (magnifying glass)
        var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            
            // Draw magnifying glass circle
            using var pen = new Pen(Color.FromArgb(0, 122, 255), 3);
            g.DrawEllipse(pen, 4, 4, 18, 18);
            
            // Draw handle
            g.DrawLine(pen, 19, 19, 27, 27);
        }
        
        return Icon.FromHandle(bitmap.GetHicon());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            if (_mainWindow != null)
            {
                _mainWindow.StateChanged -= OnWindowStateChanged;
                _mainWindow.Closing -= OnWindowClosing;
            }

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
