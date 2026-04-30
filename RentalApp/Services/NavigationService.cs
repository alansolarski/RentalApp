namespace RentalApp.Services;
using RentalApp.Database.Services;

/// <summary>
/// MAUI Shell implementation of <see cref="INavigationService"/>. Wraps Shell.Current.GoToAsync.
/// </summary>
/// <remarks>
/// This can't be unit tested because Shell.Current is null outside of a running MAUI application.
/// Tests mock INavigationService instead. NavigationService is excluded from coverage in
/// coverlet.runsettings for the same reason.
///
/// Registered as a singleton in MauiProgram because Shell is a singleton in MAUI anyway.
/// </remarks>
public class NavigationService : INavigationService
{
    /// <summary>Navigates to the given Shell route string.</summary>
    public async Task NavigateToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }

    /// <summary>Navigates to a route and passes query parameters to the target page.</summary>
    public async Task NavigateToAsync(string route, Dictionary<string, object> parameters)
    {
        await Shell.Current.GoToAsync(route, parameters);
    }

    /// <summary>Goes back one level — equivalent to Shell.Current.GoToAsync("..").</summary>
    public async Task NavigateBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>Navigates to the login route at the root of the Shell hierarchy.</summary>
    public async Task NavigateToRootAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }

    /// <summary>Pops to the root page using the navigation stack directly.</summary>
    public async Task PopToRootAsync()
    {
        await Shell.Current.Navigation.PopToRootAsync();
    }
}
