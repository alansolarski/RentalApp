using RentalApp.ViewModels;
using RentalApp.Views;

namespace RentalApp;

/// <summary>
/// Application entry point. Registers all Shell routes and resolves the initial window.
/// </summary>
/// <remarks>
/// Shell.Current.GoToAsync only works for routes registered here or in AppShell.
/// The DI-based CreateWindow approach is needed because AppShell depends on AppShellViewModel,
/// which has its own dependencies — we can't just do "new AppShell()" without the container.
/// </remarks>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();

        // Register all Shell routes so GoToAsync("PageName") works from anywhere.
        Routing.RegisterRoute(nameof(Views.MainPage), typeof(Views.MainPage));
        Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
        Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
        Routing.RegisterRoute(nameof(Views.UserListPage), typeof(Views.UserListPage));
        Routing.RegisterRoute(nameof(Views.UserDetailPage), typeof(Views.UserDetailPage));
        Routing.RegisterRoute(nameof(Views.TempPage), typeof(Views.TempPage));
        Routing.RegisterRoute(nameof(Views.ItemsListPage), typeof(Views.ItemsListPage));
        Routing.RegisterRoute(nameof(Views.CreateItemPage), typeof(Views.CreateItemPage));
        Routing.RegisterRoute(nameof(Views.ItemDetailPage), typeof(Views.ItemDetailPage));
        Routing.RegisterRoute("EditItemPage", typeof(EditItemPage));
        Routing.RegisterRoute("NearbyItemsPage", typeof(NearbyItemsPage));
        Routing.RegisterRoute("RentalsPage", typeof(RentalsPage));
        Routing.RegisterRoute(nameof(ReviewsPage), typeof(ReviewsPage));
        Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Resolve AppShell from DI so its constructor dependencies get injected.
        // The original commented-out code used "new AppShell()" which would bypass DI.
        var shell = _serviceProvider.GetService<AppShell>();
        if (shell == null)
        {
            throw new InvalidOperationException("AppShell could not be resolved from the service provider.");
        }
        var window = new Window(shell);
        return window;
    }
}
