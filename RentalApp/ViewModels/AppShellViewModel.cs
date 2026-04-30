using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Services;
using RentalApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RentalApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Shell navigation menu. Manages logout, profile/settings navigation,
    /// and updates command states when the user logs in or out.
    /// </summary>
    public partial class AppShellViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authService;
        private readonly INavigationService _navigationService;

        /// <summary>Menu bar items that can be added or removed at runtime. Not currently populated.</summary>
        public ObservableCollection<MenuBarItem> DynamicMenuBarItems { get; } = new();

        /// <summary>Parameterless constructor needed for XAML design-time support.</summary>
        public AppShellViewModel()
        {
            Title = "RentalApp";
        }

        /// <summary>Creates the ViewModel with auth and navigation services.</summary>
        public AppShellViewModel(IAuthenticationService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            // Listen for login/logout so we can refresh command can-execute states.
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
            Title = "RentalApp";
        }

        // These CanExecute methods are wired to specific role checks.
        // They're not fully used yet — most navigation is unconditional at the moment.
        private bool CanExecuteGuestAction() => _authService.HasRole("Guest");
        private bool CanExecuteUserAction() => _authService.HasRole("OrdinaryUser");
        private bool CanExecuteAdminAction() => _authService.HasRole("Admin");
        private bool CanExecuteAuthenticatedAction() => _authService.IsAuthenticated;

        /// <summary>
        /// Fires when the user logs in or out. Refreshes all command states so the
        /// logout button and nav items enable/disable correctly.
        /// </summary>
        private void OnAuthenticationStateChanged(object? sender, bool isAuthenticated)
        {
            LogoutCommand.NotifyCanExecuteChanged();
            NavigateToProfileCommand.NotifyCanExecuteChanged();
            NavigateToSettingsCommand.NotifyCanExecuteChanged();
            Debug.WriteLine($"Authentication state changed: {isAuthenticated}");
            Debug.WriteLine($"Current user is admin: {_authService.HasRole("Admin")}");
        }

        // Profile and Settings navigation goes to TempPage for now — not yet implemented as real pages.
        [RelayCommand]
        private async Task NavigateToProfileAsync()
        {
            await _navigationService.NavigateToAsync("TempPage");
        }

        [RelayCommand]
        private async Task NavigateToSettingsAsync()
        {
            await _navigationService.NavigateToAsync("TempPage");
        }

        /// <summary>
        /// Logs the user out and navigates to the login page.
        /// The CanExecute guard prevents this running before login.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanExecuteAuthenticatedAction))]
        private async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            await _navigationService.NavigateToAsync("LoginPage");

            // Notify all nav commands to re-evaluate their can-execute state.
            LogoutCommand.NotifyCanExecuteChanged();
            NavigateToProfileCommand.NotifyCanExecuteChanged();
            NavigateToSettingsCommand.NotifyCanExecuteChanged();
        }
    }
}
