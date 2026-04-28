using System.Net.Http.Json;
using RentalApp.Database.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RentalApp.Database.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly TokenStore _tokenStore;

    public ApiService(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
        };
    }

    private void AttachToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = _tokenStore.Token != null
            ? new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token)
            : null;
    }

    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
        AttachToken();
        var response = await _httpClient.GetFromJsonAsync<ItemsResponse>("items");
        return response?.Items ?? [];
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        AttachToken();
        return await _httpClient.GetFromJsonAsync<Item>($"items/{id}");
    }

    public async Task<Item?> CreateItemAsync(Item item)
    {
        AttachToken();
        var response = await _httpClient.PostAsJsonAsync("items", item);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Item>();
        return null;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<CategoriesResponse>("categories");
        return response?.Categories ?? [];
    }

    public async Task<bool> UpdateItemAsync(int id, UpdateItemRequest request)
    {
        AttachToken();
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"items/{id}", content);
        return response.IsSuccessStatusCode;
    }

    private class NearbyItemsResponse
    {
        [JsonPropertyName("items")]
        public List<NearbyItem> Items { get; set; } = new();
    }

    public async Task<List<NearbyItem>> GetNearbyItemsAsync(double lat, double lon, double radiusKm = 5)
    {
        var response = await _httpClient.GetAsync(
            $"items/nearby?lat={lat}&lon={lon}&radius={radiusKm}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<NearbyItemsResponse>(json, _jsonOptions);
        return result?.Items ?? new List<NearbyItem>();
    }

    private class RentalsResponse
    {
        [JsonPropertyName("rentals")]
        public List<Rental> Rentals { get; set; } = new();
    }

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

        return (false, $"Failed to create rental: {response.StatusCode}");
    }

    public async Task<List<Rental>> GetIncomingRentalsAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("rentals/incoming");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RentalsResponse>(json, _jsonOptions);
        return result?.Rentals ?? new List<Rental>();
    }

    public async Task<List<Rental>> GetOutgoingRentalsAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("rentals/outgoing");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RentalsResponse>(json, _jsonOptions);
        return result?.Rentals ?? new List<Rental>();
    }

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

    public async Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId)
    {
        AttachToken();
        var response = await _httpClient.GetAsync($"items/{itemId}/reviews");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ItemReviewsResponse>(json, _jsonOptions);
        return result?.Reviews ?? Enumerable.Empty<Review>();
    }

    public async Task<Review> SubmitReviewAsync(int rentalId, int rating, string? comment)
    {
        AttachToken();
        var body = new { rentalId, rating, comment };
        var response = await _httpClient.PostAsJsonAsync("reviews", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Review>()
            ?? throw new Exception("Failed to parse review response");
    }

    public async Task<UserProfile?> GetCurrentUserProfileAsync()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("users/me");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserProfile>(json, _jsonOptions);
    }

    // Add this private response class at the bottom of ApiService.cs
    private class ItemReviewsResponse
    {
        public List<Review> Reviews { get; set; } = new();
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}

internal class ItemsResponse
{
    public List<Item> Items { get; set; } = [];
}

internal class CategoriesResponse
{
    public List<Category> Categories { get; set; } = [];
}