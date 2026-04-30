using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Profile page. Loads the user's API profile (average rating,
/// items listed, rentals completed) and handles the password change form.
/// </summary>
public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly IApiService _apiService;

    [ObservableProperty]
    private User? currentUser;

    [ObservableProperty]
    private UserProfile? userProfile;

    // Password change form fields.
    [ObservableProperty]
    private string currentPassword = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string confirmNewPassword = string.Empty;

    /// <summary>True when the password change form is expanded.</summary>
    [ObservableProperty]
    private bool isChangingPassword;

    [ObservableProperty]
    private bool isLoadingProfile;

    public ProfileViewModel(
        IAuthenticationService authService,
        INavigationService navigationService,
        IApiService apiService)
    {
        _authService = authService;
        _navigationService = navigationService;
        _apiService = apiService;
        Title = "Profile";
        // Show whatever we already have from the auth service as a fallback while the API call runs.
        CurrentUser = _authService.CurrentUser;
    }

    /// <summary>
    /// Loads the enriched user profile from GET /users/me. Non-critical — if it fails
    /// the page still shows the local user data from CurrentUser.
    /// </summary>
    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        IsLoadingProfile = true;
        try
        {
            UserProfile = await _apiService.GetCurrentUserProfileAsync();
        }
        catch (Exception)
        {
            // Profile load failure is non-critical — page still shows local user data
        }
        finally
        {
            IsLoadingProfile = false;
        }
    }

    /// <summary>
    /// Validates and submits the password change request.
    /// ApiAuthenticationService.ChangePasswordAsync always returns false because the API
    /// doesn't support password changes. The error message reflects that clearly.
    /// </summary>
    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (IsBusy) return;
        if (!ValidatePasswordChange()) return;

        try
        {
            IsBusy = true;
            ClearError();

            var success = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "Password changed successfully!", "OK");
                ClearPasswordFields();
                IsChangingPassword = false;
            }
            else
            {
                // The API doesn't support password changes in this version — say so clearly
                // rather than showing a generic error.
                SetError("Password change is not supported by the API in this version.");
            }
        }
        catch (Exception ex)
        {
            SetError($"Password change failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Toggles the password change form. Clears the fields and any errors when collapsing.</summary>
    [RelayCommand]
    private void TogglePasswordChangeMode()
    {
        IsChangingPassword = !IsChangingPassword;
        if (!IsChangingPassword)
        {
            ClearPasswordFields();
            ClearError();
        }
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }

    /// <summary>Validates all password change fields before hitting the API.</summary>
    private bool ValidatePasswordChange()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            SetError("Current password is required");
            return false;
        }
        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            SetError("New password is required");
            return false;
        }
        if (NewPassword.Length < 6)
        {
            SetError("New password must be at least 6 characters long");
            return false;
        }
        if (NewPassword != ConfirmNewPassword)
        {
            SetError("New passwords do not match");
            return false;
        }
        if (CurrentPassword == NewPassword)
        {
            SetError("New password must be different from current password");
            return false;
        }
        return true;
    }

    private void ClearPasswordFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
    }
}
