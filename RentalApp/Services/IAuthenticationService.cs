using RentalApp.Database.Models;

namespace RentalApp.Services;

/// <summary>
/// Contract for authentication operations. Two implementations exist:
/// - <see cref="AuthenticationService"/> — local DB via EF Core + BCrypt (StarterApp original).
/// - <see cref="ApiAuthenticationService"/> — REST API calls (the one actually registered and used).
/// </summary>
public interface IAuthenticationService
{
    /// <summary>Fires whenever the user logs in or out. Bool parameter is true if now authenticated.</summary>
    event EventHandler<bool>? AuthenticationStateChanged;

    /// <summary>True if the user is currently logged in.</summary>
    bool IsAuthenticated { get; }

    /// <summary>The currently logged-in user, or null.</summary>
    User? CurrentUser { get; }

    /// <summary>The current user's roles. Empty in the API flow — not returned by the login endpoint.</summary>
    List<string> CurrentUserRoles { get; }

    /// <summary>Attempts to log in with the given credentials.</summary>
    Task<AuthenticationResult> LoginAsync(string email, string password);

    /// <summary>Registers a new user account.</summary>
    Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password);

    /// <summary>Logs out the current user and clears stored credentials.</summary>
    Task LogoutAsync();

    /// <summary>Returns true if the current user has the named role.</summary>
    bool HasRole(string roleName);

    /// <summary>Returns true if the current user has at least one of the named roles.</summary>
    bool HasAnyRole(params string[] roleNames);

    /// <summary>Returns true if the current user has all of the named roles.</summary>
    bool HasAllRoles(params string[] roleNames);

    /// <summary>
    /// Changes the user's password. Returns false if the current password is wrong,
    /// the user isn't logged in, or the API doesn't support it (ApiAuthenticationService always returns false).
    /// </summary>
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
}
