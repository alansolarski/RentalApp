using System.Net.Http.Json;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.Services;

/// <summary>
/// The authentication service actually used at runtime. Calls the REST API instead of
/// querying the local Postgres database directly.
/// </summary>
/// <remarks>
/// There are two implementations of <see cref="IAuthenticationService"/>:
/// - <c>AuthenticationService</c> — the original StarterApp implementation that goes straight
///   to the local database via EF Core. Still registered and wired up, but superseded.
/// - <c>ApiAuthenticationService</c> — this class. It calls POST /auth/token and POST /auth/register.
///   This is the one registered in MauiProgram as the singleton IAuthenticationService.
///
/// TokenStore is injected here so we can write the JWT after a successful login, and
/// ApiService can read it from the same singleton without depending on this class.
/// That arrangement breaks what would otherwise be a circular constructor dependency.
///
/// Note: HasRole / HasAnyRole / HasAllRoles all return false because the API login response
/// doesn't include role information. Role checks are only meaningful in the local DB flow
/// (AuthenticationService), which isn't the main path anymore.
/// </remarks>
public class ApiAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly TokenStore _tokenStore;
    private User? _currentUser;

    public event EventHandler<bool>? AuthenticationStateChanged;

    /// <summary>True if there's a token in TokenStore — i.e. the user logged in successfully.</summary>
    public bool IsAuthenticated => _tokenStore.Token != null;

    /// <summary>Minimal User object set after login. Only Email is populated — the API login response doesn't return full profile data.</summary>
    public User? CurrentUser => _currentUser;

    /// <summary>Always empty — the API doesn't return role info in the login response.</summary>
    public List<string> CurrentUserRoles => [];

    /// <summary>Creates the service with a shared TokenStore singleton.</summary>
    public ApiAuthenticationService(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
        };
    }

    /// <summary>Returns the current JWT token from TokenStore, or null if not logged in.</summary>
    public string? GetToken() => _tokenStore.Token;

    /// <summary>
    /// Posts credentials to POST /auth/token. On success, writes the JWT and user ID
    /// to TokenStore and fires AuthenticationStateChanged.
    /// </summary>
    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/token", new
            {
                email,
                password
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                // Write token and userId to the shared TokenStore so ApiService can pick them up.
                _tokenStore.SetToken(result?.Token, result?.UserId ?? 0);
                // We only get the email back from the login call — store a minimal user object.
                _currentUser = new User { Email = email };
                AuthenticationStateChanged?.Invoke(this, true);
                return new AuthenticationResult(true, "Login successful");
            }

            return new AuthenticationResult(false, "Invalid email or password");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Login failed: {ex.Message}");
        }
    }

    /// <summary>Registers a new user via POST /auth/register. Doesn't log the user in automatically.</summary>
    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register", new
            {
                firstName,
                lastName,
                email,
                password
            });

            if (response.IsSuccessStatusCode)
                return new AuthenticationResult(true, "Registration successful");

            return new AuthenticationResult(false, "Registration failed");
        }
        catch (Exception ex)
        {
            return new AuthenticationResult(false, $"Registration failed: {ex.Message}");
        }
    }

    /// <summary>Clears the token from TokenStore and fires AuthenticationStateChanged.</summary>
    public Task LogoutAsync()
    {
        _tokenStore.SetToken(null);
        _currentUser = null;
        AuthenticationStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    // Role checks aren't meaningful here because the API login response doesn't include roles.
    // These stubs exist to satisfy the IAuthenticationService contract.
    public bool HasRole(string roleName) => false;
    public bool HasAnyRole(params string[] roleNames) => false;
    public bool HasAllRoles(params string[] roleNames) => false;

    /// <summary>
    /// Password change isn't supported by the API in this version — returns false.
    /// ProfileViewModel shows a "not supported by API" message when this returns false.
    /// </summary>
    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword) => Task.FromResult(false);
}

/// <summary>Shape of the JSON response body from POST /auth/token.</summary>
internal class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
}
