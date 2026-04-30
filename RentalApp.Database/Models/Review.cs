namespace RentalApp.Database.Models;

/// <summary>
/// Represents a review as returned by GET /items/{id}/reviews and POST /reviews.
/// Not an EF Core entity — reviews are managed entirely through the API.
/// </summary>
public class Review
{
    public int Id { get; set; }

    /// <summary>The rental this review is tied to. One review per rental (enforced server-side).</summary>
    public int RentalId { get; set; }

    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>Star rating 1–5. Validation is done server-side.</summary>
    public int Rating { get; set; }

    /// <summary>Optional free-text comment. Can be null if the user only left a star rating.</summary>
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}
