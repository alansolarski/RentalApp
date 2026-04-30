namespace RentalApp.Database.Models;

/// <summary>
/// Profile data returned by GET /users/me. Not an EF Core entity — this comes
/// entirely from the API and includes aggregated stats the local DB doesn't have.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Nullable because users with no reviews yet have no rating to report.</summary>
    public decimal? AverageRating { get; set; }

    public int ItemsListed { get; set; }
    public int RentalsCompleted { get; set; }
    public DateTime? CreatedAt { get; set; }

    /// <summary>Computed full name for display.</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Formats the average rating for display in the UI.
    /// Shows a star emoji + one decimal place, or a "no ratings yet" message.
    /// </summary>
    public string RatingDisplay => AverageRating.HasValue
        ? $"⭐ {AverageRating.Value:F1}"
        : "No ratings yet";
}
