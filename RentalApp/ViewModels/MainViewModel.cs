using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Main/Dashboard page. Shows a welcome message and provides navigation
/// to user management (admin only) and other sections.
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private User? currentUser;

    [ObservableProperty]
    private string welcomeMessage = string.Empty;

    /// <summary>True if the logged-in user has the Admin role — controls admin-only UI visibility.</summary>
    [ObservableProperty]
    private bool isAdmin;

    /// <summary>Parameterless constructor for XAML design-time support.</summary>
    public MainViewModel()
    {
        Title = "Dashboard";
    }

    public MainViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Dashboard";
        LoadUserData();
    }

    /// <summary>Loads the current user from the auth service and sets the welcome message.</summary>
    private void LoadUserData()
    {
        CurrentUser = _authService.CurrentUser;
        IsAdmin = _authService.HasRole("Admin");

        if (CurrentUser != null)
        {
            WelcomeMessage = $"Welcome, {CurrentUser.FullName}!";
        }
    }

    /// <summary>Confirms logout and redirects to the Login page.</summary>
    [RelayCommand]
    private async Task LogoutAsync()
    {
        var result = await Application.Current.MainPage.DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "No");

        if (result)
        {
            await _authService.LogoutAsync();
            await _navigationService.NavigateToAsync("LoginPage");
        }
    }

    // Profile and Settings navigate to TempPage — placeholder pages not yet built out.
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

    /// <summary>Navigates to the admin User List. Blocks non-admins with an alert.</summary>
    [RelayCommand]
    private async Task NavigateToUserListAsync()
    {
        if (!IsAdmin)
        {
            await Application.Current.MainPage.DisplayAlert("Access Denied", "You don't have permission to access admin features.", "OK");
            return;
        }

        await _navigationService.NavigateToAsync("UserListPage");
    }

    /// <summary>Refreshes dashboard data. Includes an artificial 1s delay to make the spinner visible.</summary>
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        try
        {
            IsBusy = true;
            LoadUserData();
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            SetError($"Failed to refresh data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
