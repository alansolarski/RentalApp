using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

/// <summary>
/// Join table linking users to roles (maps to "user_role"). Supports soft-delete via IsActive/DeletedAt.
/// </summary>
[Table("user_role")]
[PrimaryKey(nameof(Id))]
public class UserRole
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int RoleId { get; set; }

    // Navigation properties with explicit foreign key attributes.
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Set when a role assignment is soft-deleted. Null means still active.</summary>
    public DateTime? DeletedAt { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Parameterless constructor required by EF Core.</summary>
    public UserRole()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>Convenience constructor used when assigning a role to a user.</summary>
    /// <param name="userId">The user receiving the role.</param>
    /// <param name="roleId">The role being assigned.</param>
    public UserRole(int userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>Bumps UpdatedAt. Call this before saving any change to an existing role assignment.</summary>
    public void UpdateTimestamps()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Soft-deletes this role assignment — sets DeletedAt and flips IsActive to false.</summary>
    public void MarkAsDeleted()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }

    /// <summary>Reverses a soft-delete — clears DeletedAt and sets IsActive back to true.</summary>
    public void Restore()
    {
        DeletedAt = null;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"UserRole(Id: {Id}, UserId: {UserId}, RoleId: {RoleId}, CreatedAt: {CreatedAt}, UpdatedAt: {UpdatedAt}, DeletedAt: {DeletedAt}, IsActive: {IsActive})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is UserRole other)
        {
            return Id == other.Id &&
                   UserId == other.UserId &&
                   RoleId == other.RoleId &&
                   CreatedAt == other.CreatedAt &&
                   UpdatedAt == other.UpdatedAt &&
                   DeletedAt == other.DeletedAt &&
                   IsActive == other.IsActive;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, UserId, RoleId, CreatedAt, UpdatedAt, DeletedAt, IsActive);
    }
}
