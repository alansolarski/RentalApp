using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

/// <summary>
/// Represents a user in the local Postgres database (maps to the "users" table).
/// Used by the admin user management screens and by the local AuthenticationService.
/// The API-facing auth flow (ApiAuthenticationService) doesn't need this directly —
/// it just calls POST /auth/token and stores the returned JWT.
/// </summary>
[Table("users")]
[PrimaryKey(nameof(Id))]
public class User
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string PasswordSalt { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Set when a user is soft-deleted. Null means the user is active.</summary>
    public DateTime? DeletedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property — loaded with Include() in admin queries.
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>Computed full name — not stored in the DB, used in UI bindings.</summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
