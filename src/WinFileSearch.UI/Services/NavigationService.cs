namespace WinFileSearch.UI.Services;

/// <summary>
/// Service for navigating between application pages.
/// Provides a decoupled way for ViewModels to request page navigation.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the Search page with an optional pre-filled search query.
    /// </summary>
    /// <param name="searchQuery">Optional search query to pre-fill in the search box.</param>
    void NavigateToSearch(string? searchQuery = null);

    /// <summary>
    /// Navigates to the Home page.
    /// </summary>
    void NavigateToHome();

    /// <summary>
    /// Navigates to the Settings page.
    /// </summary>
    void NavigateToSettings();

    /// <summary>
    /// Occurs when a navigation is requested. Subscribe to this event to handle page changes.
    /// </summary>
    event EventHandler<NavigationEventArgs>? NavigationRequested;
}

/// <summary>
/// Event arguments for navigation requests.
/// </summary>
public class NavigationEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the target page (e.g., "Search", "Home", "Settings").
    /// </summary>
    public string PageName { get; }

    /// <summary>
    /// Gets the optional parameter passed to the target page.
    /// </summary>
    public object? Parameter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationEventArgs"/> class.
    /// </summary>
    /// <param name="pageName">The name of the target page.</param>
    /// <param name="parameter">Optional navigation parameter.</param>
    public NavigationEventArgs(string pageName, object? parameter = null)
    {
        PageName = pageName;
        Parameter = parameter;
    }
}

/// <summary>
/// Default implementation of <see cref="INavigationService"/>.
/// </summary>
public class NavigationService : INavigationService
{
    /// <inheritdoc />
    public event EventHandler<NavigationEventArgs>? NavigationRequested;

    /// <inheritdoc />
    public void NavigateToSearch(string? searchQuery = null)
    {
        NavigationRequested?.Invoke(this, new NavigationEventArgs("Search", searchQuery));
    }

    /// <inheritdoc />
    public void NavigateToHome()
    {
        NavigationRequested?.Invoke(this, new NavigationEventArgs("Home"));
    }

    /// <inheritdoc />
    public void NavigateToSettings()
    {
        NavigationRequested?.Invoke(this, new NavigationEventArgs("Settings"));
    }
}
