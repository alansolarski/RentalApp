using Microsoft.Extensions.Logging;
using RentalApp.ViewModels;
using RentalApp.Database.Data;
using RentalApp.Views;
using System.Diagnostics;
using RentalApp.Database.Services;
using RentalApp.Services;

namespace RentalApp;

/// <summary>
/// MAUI app builder — wires up fonts, DI registrations, and the EF Core context.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // EF Core context — registered as transient because DbContext isn't thread-safe
        // and MAUI pages can navigate concurrently. Each scope gets its own instance.
        builder.Services.AddDbContext<AppDbContext>();

        // ApiAuthenticationService is the real auth implementation (calls the REST API).
        // AuthenticationService (the local DB one from the StarterApp) is not registered here.
        builder.Services.AddSingleton<IAuthenticationService, ApiAuthenticationService>();

        // NavigationService wraps Shell.Current — registered as singleton because Shell is one too.
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // Shell infrastructure — all singletons because they live for the app lifetime.
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        // Pages and ViewModels — most are transient so each navigation gets a fresh instance.
        // LoginViewModel and RegisterViewModel are singletons to preserve form state across
        // back-navigation. Everything else is transient.
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserDetailPage>();
        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddSingleton<TempViewModel>();
        builder.Services.AddTransient<TempPage>();

        // ApiService is a singleton — it holds an HttpClient which should not be recreated
        // on every request (socket exhaustion risk with transient HttpClients).
        builder.Services.AddSingleton<IApiService, ApiService>();

        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<CreateItemPage>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<EditItemViewModel>();
        builder.Services.AddTransient<EditItemPage>();

        // LocationService wraps the MAUI Geolocation API — singleton to avoid redundant permission
        // requests if multiple pages need location.
        builder.Services.AddSingleton<ILocationService, LocationService>();

        builder.Services.AddTransient<NearbyItemsViewModel>();
        builder.Services.AddTransient<NearbyItemsPage>();

        // RentalService is transient — it's a thin wrapper with no state.
        builder.Services.AddTransient<IRentalService, RentalService>();
        builder.Services.AddTransient<RentalsViewModel>();
        builder.Services.AddTransient<RentalsPage>();

        // TokenStore is the singleton that breaks the ApiService <-> ApiAuthenticationService
        // circular dependency. Both depend on it; neither depends on the other.
        builder.Services.AddSingleton<TokenStore>();

        builder.Services.AddTransient<IReviewService, ReviewService>();

        // ReviewsViewModel is a singleton because it can be navigated to from both
        // ItemDetailPage (viewing reviews) and RentalsPage (leaving a review), and we
        // don't want to lose the loaded reviews when navigating back.
        builder.Services.AddSingleton<ReviewsViewModel>();
        builder.Services.AddTransient<ReviewsPage>();

        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
