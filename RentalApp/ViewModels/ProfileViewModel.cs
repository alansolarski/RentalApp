/// @file ProfileViewModel.cs
/// @brief User profile management view model
/// @author RentalApp Development Team
/// @date 2025

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// @brief View model for the user profile page
public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly IApiService _apiService;

    [ObservableProperty]
    private User? currentUser;

    [ObservableProperty]
    private UserProfile? userProfile;

    [ObservableProperty]
    private string currentPassword = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string confirmNewPassword = string.Empty;

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
        CurrentUser = _authService.CurrentUser;
    }

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