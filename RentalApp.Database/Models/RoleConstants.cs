namespace RentalApp.Database.Models;

/// <summary>
/// String constants for role names so we don't scatter magic strings across the codebase.
/// Used in HasRole() checks and in the UserListViewModel filter options.
/// </summary>
public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string OrdinaryUser = "OrdinaryUser";
    public const string SpecialUser = "SpecialUser";

    /// <summary>All defined roles — used to populate filter dropdowns and iteration logic.</summary>
    public static readonly string[] AllRoles = { Admin, OrdinaryUser, SpecialUser };
}
