namespace RentalApp.Database.Services;

/// <summary>
/// Abstraction over MAUI Shell navigation. Lives here in RentalApp.Database rather than
/// in the MAUI project so that ViewModels in this project (and in the test project) can
/// depend on it without pulling in Android/MAUI targets.
/// </summary>
/// <remarks>
/// The concrete implementation (NavigationService) lives in RentalApp/Services/ and
/// calls Shell.Current.GoToAsync — which means it can't be unit tested. Tests mock this
/// interface instead. NavigationService is excluded from coverage for the same reason.
/// </remarks>
public interface INavigationService
{
    /// <summary>Navigates to the given Shell route.</summary>
    /// <param name="route">Route name, e.g. "ItemDetailPage" or "//login".</param>
    Task NavigateToAsync(string route);

    /// <summary>Navigates to a route and passes query parameters to the target page.</summary>
    /// <param name="route">Shell route name.</param>
    /// <param name="parameters">Key/value pairs passed as query parameters.</param>
    Task NavigateToAsync(string route, Dictionary<string, object> parameters);

    /// <summary>Navigates back one level in the Shell stack (equivalent to GoToAsync("..")).</summary>
    Task NavigateBackAsync();

    /// <summary>Navigates to the root of the Shell — used after login/logout.</summary>
    Task NavigateToRootAsync();

    /// <summary>Pops back to the root page using the navigation stack directly.</summary>
    Task PopToRootAsync();
}
