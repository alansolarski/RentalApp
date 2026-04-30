using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;
using CommunityToolkit.Mvvm.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the User Detail page. Supports creating new users and editing existing ones,
/// including role assignment and soft-delete. This is admin-only functionality — non-admins
/// get redirected to MainPage.
/// </summary>
/// <remarks>
/// This ViewModel talks directly to the local Postgres database via AppDbContext rather than
/// going through the API. That's because user management was part of the original StarterApp
/// and the API doesn't expose a user management endpoint.
///
/// Implements INotifyPropertyChanged manually rather than using ObservableObject/[ObservableProperty]
/// because the ViewModel was built before the MVVM Toolkit was wired up consistently.
/// </remarks>
[QueryProperty(nameof(UserId), "userId")]
public partial class UserDetailViewModel : INotifyPropertyChanged
{
    #region Private Fields

    private readonly AppDbContext _context;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authService;

    private int _userId;
    private User? _currentUser;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isActive = true;
    private bool _isLoading = false;
    private bool _isNewUser = false;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private ObservableCollection<RoleItem> _availableRoles = new();

    #endregion

    #region Constructor

    /// <summary>
    /// Creates the ViewModel. Commands are created here rather than via [RelayCommand]
    /// because they need CanExecute logic that depends on multiple properties.
    /// </summary>
    public UserDetailViewModel(AppDbContext context, INavigationService navigationService, IAuthenticationService authService)
    {
        _context = context;
        _navigationService = navigationService;
        _authService = authService;

        SaveUserCommand = new Command(async () => await SaveUserAsync(), CanSaveUser);
        DeleteUserCommand = new Command(async () => await DeleteUserAsync(), CanDeleteUser);
        AddRoleCommand = new Command<RoleItem>(async (role) => await AddRoleAsync(role));
        RemoveRoleCommand = new Command<RoleItem>(async (role) => await RemoveRoleAsync(role));
        BackCommand = new Command(async () => await NavigateBackAsync());

        // Refresh CanExecute on SaveUserCommand and DeleteUserCommand whenever any property changes.
        PropertyChanged += (s, e) =>
        {
            ((Command)SaveUserCommand).ChangeCanExecute();
            ((Command)DeleteUserCommand).ChangeCanExecute();
        };
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Receives the user ID from the Shell query parameter "userId".
    /// Setting this triggers the async user load in the background.
    /// UserId == 0 means "create new user".
    /// </summary>
    public int UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            OnPropertyChanged();
            _ = Task.Run(LoadUserAsync);
        }
    }

    public string FirstName
    {
        get => _firstName;
        set { _firstName = value; OnPropertyChanged(); ClearMessages(); }
    }

    public string LastName
    {
        get => _lastName;
        set { _lastName = value; OnPropertyChanged(); ClearMessages(); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); ClearMessages(); }
    }

    /// <summary>Password field — only relevant when creating a new user. Not shown for existing users.</summary>
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); ClearMessages(); }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { _confirmPassword = value; OnPropertyChanged(); ClearMessages(); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    /// <summary>True when creating a new user (UserId == 0). Drives PageTitle and ShowPasswordFields.</summary>
    public bool IsNewUser
    {
        get => _isNewUser;
        set { _isNewUser = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set { _successMessage = value; OnPropertyChanged(); }
    }

    public ObservableCollection<RoleItem> AvailableRoles
    {
        get => _availableRoles;
        set { _availableRoles = value; OnPropertyChanged(); }
    }

    public string PageTitle => IsNewUser ? "Create New User" : "Edit User";

    /// <summary>Password fields are only shown when creating a new user.</summary>
    public bool ShowPasswordFields => IsNewUser;

    /// <summary>
    /// Prevents deleting the currently logged-in admin — avoids locking yourself out.
    /// </summary>
    public bool CanDeleteCurrentUser => !IsNewUser && _currentUser?.Id != _authService.CurrentUser?.Id;

    #endregion

    #region Commands

    public ICommand SaveUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand AddRoleCommand { get; }
    public ICommand RemoveRoleCommand { get; }
    public ICommand BackCommand { get; }

    #endregion

    #region Private Methods

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }

    /// <summary>
    /// Loads the user from the local database, including their current role assignments.
    /// Redirects to MainPage if the caller isn't an admin.
    /// </summary>
    private async Task LoadUserAsync()
    {
        if (!_authService.HasRole(RoleConstants.Admin))
        {
            await _navigationService.NavigateToAsync("MainPage");
            return;
        }

        IsLoading = true;
        try
        {
            var allRoles = await _context.Roles.ToListAsync();

            if (UserId == 0)
            {
                // New user — blank out all fields and show all roles as unassigned.
                IsNewUser = true;
                _currentUser = null;
                FirstName = string.Empty;
                LastName = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                IsActive = true;

                AvailableRoles = new ObservableCollection<RoleItem>(
                    allRoles.Select(r => new RoleItem
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsAssigned = false
                    }));
            }
            else
            {
                IsNewUser = false;
                _currentUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == UserId);

                if (_currentUser == null)
                {
                    ErrorMessage = "User not found";
                    return;
                }

                FirstName = _currentUser.FirstName;
                LastName = _currentUser.LastName;
                Email = _currentUser.Email;
                IsActive = _currentUser.IsActive;

                var userRoleIds = _currentUser.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.RoleId).ToList();

                AvailableRoles = new ObservableCollection<RoleItem>(
                    allRoles.Select(r => new RoleItem
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsAssigned = userRoleIds.Contains(r.Id)
                    }));
            }

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(ShowPasswordFields));
            OnPropertyChanged(nameof(CanDeleteCurrentUser));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSaveUser()
    {
        return !IsLoading &&
               !string.IsNullOrWhiteSpace(FirstName) &&
               !string.IsNullOrWhiteSpace(LastName) &&
               !string.IsNullOrWhiteSpace(Email) &&
               (!IsNewUser || (!string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ConfirmPassword)));
    }

    private async Task SaveUserAsync()
    {
        ClearMessages();

        if (!ValidateInput())
            return;

        IsLoading = true;
        try
        {
            if (IsNewUser)
            {
                await CreateUserAsync();
            }
            else
            {
                await UpdateUserAsync();
            }

            SuccessMessage = IsNewUser ? "User created successfully!" : "User updated successfully!";

            if (IsNewUser)
            {
                await Task.Delay(1500);
                await NavigateBackAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateUserAsync()
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email.Trim());
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password, salt);

        var user = new User
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Email = Email.Trim(),
            PasswordHash = hashedPassword,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = IsActive
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign any roles already checked in the UI.
        var selectedRoles = AvailableRoles.Where(r => r.IsAssigned).ToList();
        foreach (var role in selectedRoles)
        {
            var userRole = new UserRole(user.Id, role.Id);
            _context.UserRoles.Add(userRole);
        }

        if (selectedRoles.Any())
        {
            await _context.SaveChangesAsync();
        }

        _currentUser = user;
        IsNewUser = false;
    }

    private async Task UpdateUserAsync()
    {
        if (_currentUser == null) return;

        // Prevent updating to an email already claimed by a different user.
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == Email.Trim() && u.Id != _currentUser.Id);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already used by another user");
        }

        _currentUser.FirstName = FirstName.Trim();
        _currentUser.LastName = LastName.Trim();
        _currentUser.Email = Email.Trim();
        _currentUser.IsActive = IsActive;
        _currentUser.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(_currentUser);
        await _context.SaveChangesAsync();
    }

    private bool CanDeleteUser()
    {
        return !IsLoading && !IsNewUser && CanDeleteCurrentUser;
    }

    /// <summary>
    /// Soft-deletes the user: sets IsActive=false, stamps DeletedAt, and deactivates all their roles.
    /// Asks for confirmation first because this can't be undone from the UI.
    /// </summary>
    private async Task DeleteUserAsync()
    {
        if (_currentUser == null) return;

        var result = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete user '{_currentUser.FullName}'? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (!result) return;

        IsLoading = true;
        try
        {
            _currentUser.IsActive = false;
            _currentUser.DeletedAt = DateTime.UtcNow;
            _currentUser.UpdatedAt = DateTime.UtcNow;

            // Deactivate all role assignments too so they don't show in role filters.
            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == _currentUser.Id).ToListAsync();
            foreach (var userRole in userRoles)
            {
                userRole.MarkAsDeleted();
            }

            _context.Users.Update(_currentUser);
            _context.UserRoles.UpdateRange(userRoles);
            await _context.SaveChangesAsync();

            await NavigateBackAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddRoleAsync(RoleItem role)
    {
        if (_currentUser == null || role.IsAssigned) return;

        try
        {
            var userRole = new UserRole(_currentUser.Id, role.Id);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            role.IsAssigned = true;
            SuccessMessage = $"Role '{role.Name}' added successfully!";
            await Task.Delay(1500);
            ClearMessages();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error adding role: {ex.Message}";
        }
    }

    /// <summary>Soft-deletes the role assignment rather than physically removing the row.</summary>
    private async Task RemoveRoleAsync(RoleItem role)
    {
        if (_currentUser == null || !role.IsAssigned) return;

        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == _currentUser.Id && ur.RoleId == role.Id && ur.IsActive);

            if (userRole != null)
            {
                userRole.MarkAsDeleted();
                _context.UserRoles.Update(userRole);
                await _context.SaveChangesAsync();

                role.IsAssigned = false;
                SuccessMessage = $"Role '{role.Name}' removed successfully!";
                await Task.Delay(1500);
                ClearMessages();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error removing role: {ex.Message}";
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "First name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Last name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email is required.";
            return false;
        }

        if (!IsValidEmail(Email.Trim()))
        {
            ErrorMessage = "Please enter a valid email address.";
            return false;
        }

        if (IsNewUser)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required.";
                return false;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters long.";
                return false;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return false;
            }
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private async Task NavigateBackAsync()
    {
        await _navigationService.NavigateToAsync("UserListPage");
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

/// <summary>
/// UI model for a role row in the assignment list. Tracks whether the role is currently
/// assigned and exposes ButtonText/ButtonColor for the Add/Remove button.
/// </summary>
public class RoleItem : INotifyPropertyChanged
{
    private bool _isAssigned;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// True if this role is currently assigned to the user.
    /// Setting this also notifies ButtonText and ButtonColor so the button updates.
    /// </summary>
    public bool IsAssigned
    {
        get => _isAssigned;
        set
        {
            _isAssigned = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(ButtonColor));
        }
    }

    public string ButtonText => IsAssigned ? "Remove" : "Add";
    public Color ButtonColor => IsAssigned ? Color.FromArgb("#dc3545") : Color.FromArgb("#28a745");

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
