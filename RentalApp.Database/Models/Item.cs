namespace RentalApp.Database.Models;

/// <summary>
/// Represents a rental listing. Used both for local EF Core persistence and as the
/// deserialization target for GET /items and GET /items/{id} responses.
/// </summary>
/// <remarks>
/// OwnerName is a flat string property added because binding to <c>Owner.FirstName</c>
/// required the Owner navigation property to be loaded. The API returns OwnerName directly,
/// so we just map it here instead of making an extra DB round-trip.
/// </remarks>
public class Item
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DailyRate { get; set; }
    public int CategoryId { get; set; }
    public int OwnerId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property used by EF Core when loading from local DB.
    public User? Owner { get; set; }

    // Flat owner name from the API response. Added because the API returns this directly
    // and we needed it on ItemDetailPage without loading the full Owner navigation property.
    public string? OwnerName { get; set; }
}
