using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for managing global keyboard hotkeys
/// </summary>
public interface IGlobalHotkeyService : IDisposable
{
    void Initialize(Window window);
    event EventHandler? HotkeyPressed;
}

public class GlobalHotkeyService : IGlobalHotkeyService
{
    // Win32 API imports
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    // Hotkey ID
    private const int HOTKEY_ID = 9000;

    // Modifiers
    private const uint MOD_WIN = 0x0008;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_NOREPEAT = 0x4000;

    // Virtual key codes
    private const uint VK_F = 0x46;

    // Windows message
    private const int WM_HOTKEY = 0x0312;

    private IntPtr _windowHandle;
    private HwndSource? _source;

    public event EventHandler? HotkeyPressed;

    public void Initialize(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _windowHandle = helper.Handle;

        if (_windowHandle == IntPtr.Zero)
        {
            // Window not yet loaded, defer initialization
            window.Loaded += (s, e) =>
            {
                _windowHandle = new WindowInteropHelper(window).Handle;
                RegisterHotkeyInternal();
            };
        }
        else
        {
            RegisterHotkeyInternal();
        }
    }

    private void RegisterHotkeyInternal()
    {
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(HwndHook);

        // Register Win+Shift+F
        var success = RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_WIN | MOD_SHIFT | MOD_NOREPEAT, VK_F);
        
        if (!success)
        {
            System.Diagnostics.Debug.WriteLine("Failed to register global hotkey Win+Shift+F");
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        _source?.RemoveHook(HwndHook);

        if (_windowHandle != IntPtr.Zero)
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
        }

        GC.SuppressFinalize(this);
    }
}
