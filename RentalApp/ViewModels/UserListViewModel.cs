using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;
using CommunityToolkit.Mvvm.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the User List page. Loads all active users from the local database,
/// supports role filtering and text search, and navigates to user detail or creation.
/// </summary>
/// <remarks>
/// Admin-only. Non-admins are redirected to MainPage before the data loads.
/// Talks directly to AppDbContext because user management is a local DB feature —
/// the API doesn't expose user admin endpoints.
/// </remarks>
public partial class UserListViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _context;
    private readonly INavigationService _navigationService;
    private readonly IAuthenticationService _authenticationService;

    private ObservableCollection<UserListItem> _users = new();
    private ObservableCollection<UserListItem> _filteredUsers = new();
    private string _selectedRoleFilter = "All";
    private string _searchText = string.Empty;
    private bool _isLoading = false;
    private bool _isRefreshing = false;

    /// <summary>
    /// Sets up commands and kicks off the initial user load in the background.
    /// Also populates the role filter options from RoleConstants.
    /// </summary>
    public UserListViewModel(AppDbContext context, INavigationService navigationService, IAuthenticationService authenticationService)
    {
        _context = context;
        _navigationService = navigationService;
        _authenticationService = authenticationService;

        LoadUsersCommand = new Command(async () => await LoadUsersAsync());
        RefreshCommand = new Command(async () => await RefreshUsersAsync());
        UserSelectedCommand = new Command<UserListItem>(async (user) => await NavigateToUserDetailAsync(user));
        CreateUserCommand = new Command(async () => await NavigateToCreateUserAsync());

        // Build the role filter dropdown: "All" plus each defined role name.
        RoleFilterOptions = new ObservableCollection<string> { "All" };
        foreach (var role in RoleConstants.AllRoles)
        {
            RoleFilterOptions.Add(role);
        }

        // Start loading immediately — don't wait for OnAppearing.
        _ = Task.Run(LoadUsersAsync);
    }

    /// <summary>All users loaded from the DB. ApplyFilters reads from this to build FilteredUsers.</summary>
    public ObservableCollection<UserListItem> Users
    {
        get => _users;
        set { _users = value; OnPropertyChanged(); }
    }

    /// <summary>Filtered subset of Users shown in the CollectionView.</summary>
    public ObservableCollection<UserListItem> FilteredUsers
    {
        get => _filteredUsers;
        set { _filteredUsers = value; OnPropertyChanged(); }
    }

    /// <summary>"All" plus all role names from RoleConstants. Populated once in the constructor.</summary>
    public ObservableCollection<string> RoleFilterOptions { get; }

    /// <summary>Setting this triggers ApplyFilters immediately.</summary>
    public string SelectedRoleFilter
    {
        get => _selectedRoleFilter;
        set { _selectedRoleFilter = value; OnPropertyChanged(); ApplyFilters(); }
    }

    /// <summary>Setting this triggers ApplyFilters immediately. Case-insensitive search on name, email, roles.</summary>
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    /// <summary>True during pull-to-refresh. Separate from IsLoading to drive the RefreshView indicator.</summary>
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set { _isRefreshing = value; OnPropertyChanged(); }
    }

    public bool IsAdmin => _authenticationService.HasRole(RoleConstants.Admin);

    public ICommand LoadUsersCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand UserSelectedCommand { get; }
    public ICommand CreateUserCommand { get; }

    /// <summary>
    /// Fetches all active users with their roles from the local DB and applies current filters.
    /// Redirects to MainPage if the caller isn't an admin.
    /// </summary>
    private async Task LoadUsersAsync()
    {
        if (!IsAdmin)
        {
            await _navigationService.NavigateToAsync("//MainPage");
            return;
        }

        IsLoading = true;
        try
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var userItems = users.Select(u => new UserListItem
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                FullName = u.FullName,
                CreatedAt = u.CreatedAt ?? DateTime.MinValue,
                IsActive = u.IsActive,
                Roles = u.UserRoles
                    .Where(ur => ur.IsActive)
                    .Select(ur => ur.Role.Name)
                    .ToList(),
                RolesDisplay = string.Join(", ", u.UserRoles
                    .Where(ur => ur.IsActive)
                    .Select(ur => ur.Role.Name))
            }).ToList();

            Users = new ObservableCollection<UserListItem>(userItems);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshUsersAsync()
    {
        IsRefreshing = true;
        await LoadUsersAsync();
        IsRefreshing = false;
    }

    /// <summary>
    /// Filters the Users collection by role and search text and updates FilteredUsers.
    /// Called automatically whenever SelectedRoleFilter or SearchText changes.
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = Users.AsEnumerable();

        if (SelectedRoleFilter != "All")
        {
            filtered = filtered.Where(u => u.Roles.Contains(SelectedRoleFilter));
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(u =>
                u.FullName.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower) ||
                u.RolesDisplay.ToLower().Contains(searchLower));
        }

        FilteredUsers = new ObservableCollection<UserListItem>(filtered);
    }

    [RelayCommand]
    private async Task NavigateToDashboardAsync()
    {
        await _navigationService.NavigateToAsync("MainPage");
    }

    private async Task NavigateToUserDetailAsync(UserListItem user)
    {
        if (user != null)
        {
            await _navigationService.NavigateToAsync($"UserDetailPage?userId={user.Id}");
        }
    }

    /// <summary>Navigates to UserDetailPage with userId=0 to trigger new-user creation mode.</summary>
    private async Task NavigateToCreateUserAsync()
    {
        await _navigationService.NavigateToAsync("UserDetailPage?userId=0");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Flat projection of a User for display in the list. Avoids passing the full EF Core
/// entity to the XAML and makes role display easy to bind.
/// </summary>
public class UserListItem
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    /// <summary>List of role names used for role filter matching.</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>Comma-separated role names for display and text search.</summary>
    public string RolesDisplay { get; set; } = string.Empty;
}
