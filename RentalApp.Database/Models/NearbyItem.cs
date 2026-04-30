using System.Text.Json.Serialization;

namespace RentalApp.Database.Models;

/// <summary>
/// Deserialization target for items returned by GET /items/nearby.
/// Separate from <see cref="Item"/> because the nearby endpoint returns extra
/// fields (Distance, AverageRating) and omits others (OwnerId, CategoryId).
/// </summary>
/// <remarks>
/// Distance and AverageRating are nullable because the API can return null for them
/// (e.g. no reviews yet, or distance calculation failed). Had to change these from
/// decimal to decimal? after getting silent null-to-zero coercion bugs.
/// </remarks>
public class NearbyItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("dailyRate")]
    public decimal DailyRate { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("ownerName")]
    public string OwnerName { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    // Nullable — the API can omit this if distance calculation fails.
    [JsonPropertyName("distance")]
    public decimal? Distance { get; set; }

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    // Nullable — items with no reviews yet return null, not 0.
    [JsonPropertyName("averageRating")]
    public decimal? AverageRating { get; set; }
}
