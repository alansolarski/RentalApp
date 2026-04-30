namespace RentalApp.Database.Services;

/// <summary>
/// Singleton that holds the current user's JWT token and user ID.
/// </summary>
/// <remarks>
/// This exists to break a circular dependency. ApiService needs the token to attach
/// to requests, but ApiAuthenticationService needs ApiService to get the token in the
/// first place. Giving both of them a shared TokenStore lets ApiAuthenticationService
/// write the token after a successful login, and ApiService read it on every request,
/// without either of them depending on the other.
///
/// It lives here in RentalApp.Database rather than in the MAUI project because ApiService
/// also lives here (for testability), and TokenStore is a constructor parameter of ApiService.
///
/// Registered as a singleton in MauiProgram so the same instance is shared across all
/// services that need it.
/// </remarks>
public class TokenStore
{
    /// <summary>The current JWT bearer token, or null if the user isn't logged in.</summary>
    public string? Token { get; private set; }

    /// <summary>The numeric user ID extracted from the login response.</summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Sets the token and user ID after a successful login, or clears them on logout.
    /// </summary>
    /// <param name="token">The JWT from the API, or null to clear on logout.</param>
    /// <param name="userId">The user's numeric ID. Defaults to 0 when clearing.</param>
    public void SetToken(string? token, int userId = 0)
    {
        Token = token;
        UserId = userId;
    }
}
