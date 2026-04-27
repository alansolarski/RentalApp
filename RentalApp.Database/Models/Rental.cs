using System.Text.Json.Serialization;

namespace RentalApp.Database.Models;

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

    [JsonPropertyName("approvedAt")]
    public DateTime? ApprovedAt { get; set; }
}