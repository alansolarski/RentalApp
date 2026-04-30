namespace RentalApp.Database.Models;

/// <summary>
/// Category returned by GET /categories. Used to populate the picker when creating an item.
/// Not persisted locally — we always fetch fresh from the API.
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly slug, e.g. "power-tools". Returned by the API but not displayed in the UI right now.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Number of items in this category. Returned by the API but not displayed in the UI right now.</summary>
    public int ItemCount { get; set; }
}
