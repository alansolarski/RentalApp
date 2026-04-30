using System.Text.Json.Serialization;

namespace RentalApp.Database.Models;

/// <summary>
/// Represents a rental record as returned by GET /rentals/incoming and GET /rentals/outgoing.
/// This is API-shaped — it's not an EF Core entity. Rentals are managed entirely through
/// the API, not the local database.
/// </summary>
/// <remarks>
/// ApprovedAt is nullable because it's only set after an owner approves the request.
/// JsonPropertyName attributes are here because the API uses camelCase, and I originally
/// didn't have PropertyNameCaseInsensitive = true on the deserializer options — so some
/// fields were silently dropping to defaults. They're kept explicit for safety.
/// </remarks>
public class Rental
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("itemId")]
    public int ItemId { get; set; }

    [JsonPropertyName("itemTitle")]
    public string ItemTitle { get; set; } = string.Empty;

    [JsonPropertyName("borrowerId")]
    public int BorrowerId { get; set; }

    [JsonPropertyName("borrowerName")]
    public string BorrowerName { get; set; } = string.Empty;

    [JsonPropertyName("ownerId")]
    public int OwnerId { get; set; }

    [JsonPropertyName("ownerName")]
    public string OwnerName { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; }

    [JsonPropertyName("requestedAt")]
    public DateTime RequestedAt { get; set; }

    // Only populated once the owner approves — null before that.
    [JsonPropertyName("approvedAt")]
    public DateTime? ApprovedAt { get; set; }
}
