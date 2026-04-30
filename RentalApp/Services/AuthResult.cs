using RentalApp.Database.Models;

namespace RentalApp.Services;

/// <summary>
/// Result object from an authentication operation. Part of the original StarterApp
/// and used by the local-DB AuthenticationService flow.
/// </summary>
/// <remarks>
/// The API flow (ApiAuthenticationService) uses <see cref="AuthenticationResult"/> instead.
/// This class is kept for compatibility with any code that still references the old flow.
/// </remarks>
public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>The authenticated user, or null on failure.</summary>
    public User? User { get; set; }

    /// <summary>The user's roles at time of login.</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>Creates a successful result with user and roles populated.</summary>
    public static AuthResult Success(User user, List<string> roles)
    {
        return new AuthResult
        {
            IsSuccess = true,
            User = user,
            Roles = roles
        };
    }

    /// <summary>Creates a failure result with the given error message.</summary>
    public static AuthResult Failure(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
