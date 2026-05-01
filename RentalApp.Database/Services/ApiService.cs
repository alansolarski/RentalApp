using System.Net.Http.Json;
using RentalApp.Database.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RentalApp.Database.Services;

/// <summary>
/// HTTP client wrapper for the rental API (https://set09102-api.b-davison.workers.dev/).
/// Handles all network calls — items, categories, rentals, reviews, and profile.
/// </summary>
/// <remarks>
/// This class lives in RentalApp.Database rather than the MAUI project so that the
/// interfaces it implements (IApiService) can be mocked in tests without referencing
/// net10.0-android targets. ApiService itself is excluded from code coverage in
/// coverlet.runsettings because it makes live HTTP calls that can't be unit tested.
///
/// TokenStore is injected rather than having ApiService call ApiAuthenticationService
/// directly — that would create a circular dependency because ApiAuthenticationService
/// also depends on this class. TokenStore acts as the shared token holder between them.
/// </remarks>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly TokenStore _tokenStore;

    /// <summary>Creates a new ApiService. TokenStore is a singleton shared with ApiAuthenticationService.</summary>
    public ApiService(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
        };
    }

    /// <summary>
    /// Attaches the current JWT token as a Bearer header before each request.
    /// If the user isn't logged in, the header is cleared so we don't send a stale token.
    /// </summary>
    private void AttachToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = _tokenStore.Token != null
            ? new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token)
            : null;
    }

    // ----- Items -----

    /// <summary>Returns all items from GET /items.</summary>
    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
        AttachToken();
        var response = await _httpClient.GetFromJsonAsync<ItemsResponse>("items?pageSize=100");
        return response?.Items ?? [];
    }

    /// <summary>Returns a single item from GET /items/{id}, or null if not found.</summary>
    public async Task<Item?> GetItemByIdAsync(int id)
    {
        AttachToken();
        return await _httpClient.GetFromJsonAsync<Item>($"items/{id}");
    }

    /// <summary>Creates a new item via POST /items. Returns the server response or null on failure.</summary>
    public async Task<Item?> CreateItemAsync(Item item)
    {
        AttachToken();
        var response = await _httpClient.PostAsJsonAsync("items", item);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Item>();
        return null;
    }

    /// <summary>Returns all categories from GET /categories. No token needed — public endpoint.</summary>
    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<CategoriesResponse>("categories");
        return response?.Categories ?? [];
    }

    /// <summary>Updates an item via PUT /items/{id}. Returns true if the server accepted the change.</summary>
    public async Task<bool> UpdateItemAsync(int id, UpdateItemRequest request)
    {
        AttachToken();
        // Using JsonSerializer explicitly here because UpdateItemRequest has JsonPropertyName
        // attributes that GetFromJsonAsync/PostAsJsonAsync respect, but PutAsync needs manual serialization.
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"items/{id}", content);
        return response.IsSuccessStatusCode;
    }

    // Private wrapper class for the /items/nearby response envelope.
    private class NearbyItemsResponse
    {
        [JsonPropertyName("items")]
        public List<NearbyItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Returns items near the given coordinates from GET /items/nearby.
    /// Uses _jsonOptions with PropertyNameCaseInsensitive because the nearby endpoint
    /// returns camelCase JSON and some fields (distance, averageRating) are nullable.
    /// </summary>
    public async Task<List<NearbyItem>> GetNearbyItemsAsync(double lat, double lon, double radiusKm = 5)
    {
        var response = await _httpClient.GetAsync(
            $"items/nearby?lat={lat}&lon={lon}&radius={radiusKm}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<NearbyItemsResponse>(json, _jsonOptions);
        return result?.Items ?? new List<NearbyItem>();
    }

    // Private wrapper class for rental list responses.
    private class RentalsResponse
    {
        [JsonPropertyName("rentals")]
        public List<Rental> Rentals { get; set; } = new();
    }

    /// <summary>Creates a rental request via POST /rentals. Formats dates as yyyy-MM-dd strings.</summary>
    public async Task<(bool Success, string Error)> CreateRentalAsync(
        int itemId, DateTime startDate, DateTime endDate)
    {
        AttachToken();
        var body = JsonSerializer.Serialize(new
        {
            itemId,
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd")
        });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("rentals", content);

        if (response.IsSuccessStatusCode)
            return (true, string.Empty);

        var errorBody = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest &&
            errorBody.Contains("own item", StringComparison.OrdinalIgnoreCase))
            return (false, "You can't request a rental for your own item.");

        return (false, $"Failed to create rental: {response.StatusCode}");
    }

    /// <summary>Returns incoming rentals from GET /rentals/incoming.</summary>
    public async Task<List<Rental>> GetIncomingRentalsAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("rentals/incoming");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RentalsResponse>(json, _jsonOptions);
        return result?.Rentals ?? new List<Rental>();
    }

    /// <summary>Returns outgoing rentals from GET /rentals/outgoing.</summary>
    public async Task<List<Rental>> GetOutgoingRentalsAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("rentals/outgoing");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RentalsResponse>(json, _jsonOptions);
        return result?.Rentals ?? new List<Rental>();
    }

    /// <summary>Updates a rental's status via PATCH /rentals/{id}/status.</summary>
    public async Task<(bool Success, string Error)> UpdateRentalStatusAsync(int rentalId, string status)
    {
        AttachToken();
        var body = JsonSerializer.Serialize(new { status });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync($"rentals/{rentalId}/status", content);

        if (response.IsSuccessStatusCode)
            return (true, string.Empty);

        return (false, $"Failed to update status: {response.StatusCode}");
    }

    /// <summary>Returns all reviews for an item from GET /items/{id}/reviews.</summary>
    public async Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId)
    {
        AttachToken();
        var response = await _httpClient.GetAsync($"items/{itemId}/reviews");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ItemReviewsResponse>(json, _jsonOptions);
        return result?.Reviews ?? Enumerable.Empty<Review>();
    }

    /// <summary>Submits a review via POST /reviews. Throws HttpRequestException on 4xx/5xx so
    /// ReviewService can catch and translate the status codes into user messages.</summary>
    public async Task<Review> SubmitReviewAsync(int rentalId, int rating, string? comment)
    {
        AttachToken();
        var body = new { rentalId, rating, comment };
        var response = await _httpClient.PostAsJsonAsync("reviews", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Review>()
            ?? throw new Exception("Failed to parse review response");
    }

    /// <summary>Returns the current user's profile from GET /users/me. Returns null if the request fails.</summary>
    public async Task<UserProfile?> GetCurrentUserProfileAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("users/me");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserProfile>(json, _jsonOptions);
    }

    // Wrapper for the reviews response envelope from GET /items/{id}/reviews.
    private class ItemReviewsResponse
    {
        public List<Review> Reviews { get; set; } = new();
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    // PropertyNameCaseInsensitive = true because the API uses camelCase and some endpoints
    // return fields that don't have explicit [JsonPropertyName] attributes on the model classes.
    // Without this, properties like "averageRating" would silently deserialize to their defaults.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}

// Response envelope for GET /items.
internal class ItemsResponse
{
    public List<Item> Items { get; set; } = [];
}

// Response envelope for GET /categories.
internal class CategoriesResponse
{
    public List<Category> Categories { get; set; } = [];
}
