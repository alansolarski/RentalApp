using System.Text.Json.Serialization;

namespace RentalApp.Database.Models;

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