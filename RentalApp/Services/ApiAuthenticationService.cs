using System.Net.Http.Json;
using RentalApp.Database.Models;

namespace RentalApp.Services;

public class ApiAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private string? _token;
    private User? _currentUser;

    public event EventHandler<bool>? AuthenticationStateChanged;
    public bool IsAuthenticated => _token != null;
    public User? CurrentUser => _currentUser;
    public List<string> CurrentUserRoles => [];

    public ApiAuthenticationService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
        };
    }

    public string? GetToken() => _token;

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
                _token = result?.Token;
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

    public Task LogoutAsync()
    {
        _token = null;
        _currentUser = null;
        AuthenticationStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public bool HasRole(string roleName) => false;
    public bool HasAnyRole(params string[] roleNames) => false;
    public bool HasAllRoles(params string[] roleNames) => false;
    public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword) => Task.FromResult(false);
}

internal class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
}