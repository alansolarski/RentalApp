using System.Net.Http.Json;
using RentalApp.Database.Models;

namespace RentalApp.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
        };
    }

    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ItemsResponse>("items");
        return response?.Items ?? [];
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Item>($"items/{id}");
    }

    public async Task<Item?> CreateItemAsync(Item item)
    {
        var response = await _httpClient.PostAsJsonAsync("items", item);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Item>();
        return null;
    }
}

// Response wrapper to match API format
internal class ItemsResponse
{
    public List<Item> Items { get; set; } = [];
}