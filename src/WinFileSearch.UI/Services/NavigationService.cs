namespace WinFileSearch.UI.Services;

/// <summary>
/// Navigation service for switching between pages
/// </summary>
public interface INavigationService
{
    void NavigateToSearch(string? searchQuery = null);
    void NavigateToHome();
    void NavigateToSettings();
    
    event EventHandler<NavigationEventArgs>? NavigationRequested;
}

public class NavigationEventArgs : EventArgs
{
    public string PageName { get; }
    public object? Parameter { get; }

    public NavigationEventArgs(string pageName, object? parameter = null)
    {
        PageName = pageName;
        Parameter = parameter;
    }
}

public class NavigationService : INavigationService
{
    public event EventHandler<NavigationEventArgs>? NavigationRequested;

    public void NavigateToSearch(string? searchQuery = null)
    {
        NavigationRequested?.Invoke(this, new NavigationEventArgs("Search", searchQuery));
    }

    public void NavigateToHome()
    {
        NavigationRequested?.Invoke(this, new NavigationEventArgs("Home"));
    }

    public void NavigateToSettings()
    {
        NavigationRequested?.Invoke(this, new NavigationEventArgs("Settings"));
    }
}
