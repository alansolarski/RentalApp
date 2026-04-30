using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Services;
using RentalApp.Services;
using RentalApp.Views;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Login page. Handles email/password input, calls the auth service,
/// and navigates to the items list on success.
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    /// <summary>RememberMe is tracked but not actually implemented — the API doesn't support it.</summary>
    [ObservableProperty]
    private bool rememberMe;

    /// <summary>
    /// Separate IsBusy field that also notifies LoginCommand to re-evaluate its can-execute.
    /// Used to prevent double-submission if the user taps Login twice quickly.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isBusy;

    /// <summary>Parameterless constructor needed for XAML design-time support.</summary>
    public LoginViewModel()
    {
        Title = "Login";
    }

    public LoginViewModel(IAuthenticationService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        Title = "Login";
    }

    /// <summary>
    /// Submits the login credentials. Guards against re-entry with IsBusy.
    /// Navigates to ItemsListPage on success, shows an error message on failure.
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter both email and password");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _authService.LoginAsync(Email, Password);

            if (result.IsSuccess)
            {
                await _navigationService.NavigateToAsync("ItemsListPage");
            }
            else
            {
                SetError(result.Message);
            }
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Navigates to the Registration page.</summary>
    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await _navigationService.NavigateToAsync("RegisterPage");
    }

    /// <summary>Placeholder for forgot password — not implemented by the API.</summary>
    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        // TODO: implement if the API ever adds a password reset endpoint
        await Application.Current.MainPage.DisplayAlert("Info", "Forgot password functionality not implemented yet", "OK");
    }
}
