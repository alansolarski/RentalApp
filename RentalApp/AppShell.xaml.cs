using RentalApp.ViewModels;
using RentalApp.Views;

namespace RentalApp;

/// <summary>
/// Shell root page. Binds to AppShellViewModel for the top-level navigation menu
/// and logout command.
/// </summary>
/// <remarks>
/// AppShellViewModel is a singleton in DI so the same instance persists across all
/// page navigations — that's how the navigation menu stays in sync with auth state.
/// </remarks>
public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Additional route registration — some routes are registered here, others in App.xaml.cs.
        // Both approaches work; this one exists from the original StarterApp structure.
        Routing.RegisterRoute(nameof(ItemsListPage), typeof(ItemsListPage));
    }
}
