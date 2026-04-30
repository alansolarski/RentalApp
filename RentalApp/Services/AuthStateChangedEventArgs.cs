using RentalApp.Database.Models;

namespace RentalApp.Services;

/// <summary>
/// Event args for authentication state change events. Part of the original StarterApp.
/// Not currently used by ApiAuthenticationService — that fires EventHandler&lt;bool&gt; directly.
/// Kept for compatibility with anything that might listen to the old event signature.
/// </summary>
public class AuthStateChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; set; }
    public User? User { get; set; }
    public List<string> Roles { get; set; } = new();
}
