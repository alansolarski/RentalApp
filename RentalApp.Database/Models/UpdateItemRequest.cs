using System.Text.Json.Serialization;

namespace RentalApp.Database.Models;

/// <summary>
/// Request body sent to PUT /items/{id}. Intentionally limited to fields the owner
/// is allowed to change — we don't let them reassign OwnerId or CategoryId through this endpoint.
/// </summary>
public class UpdateItemRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("dailyRate")]
    public decimal DailyRate { get; set; }

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }
}
