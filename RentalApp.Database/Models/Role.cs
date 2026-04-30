using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

/// <summary>
/// Represents a role in the local database (maps to the "role" table).
/// Used by the admin user management screens to assign/remove roles.
/// </summary>
[Table("role")]
[PrimaryKey(nameof(Id))]
public class Role
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>If true, this role is automatically assigned to new registrations.</summary>
    public bool IsDefault { get; set; } = false;

    // Navigation property back to all user-role assignments for this role.
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
