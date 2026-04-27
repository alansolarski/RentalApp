using System.Net.Http.Json;
using RentalApp.Database.Models;
using System.Text;
using System.Text.Json;

namespace RentalApp.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;

    public ApiService(IAuthenticationService authService)
    {
        _authService = authService;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
        };
    }

    private void SetAuthHeader()
    {
        if (_authService is ApiAuthenticationService apiAuth && apiAuth.GetToken() != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiAuth.GetToken());
        }
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
        SetAuthHeader();
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
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"items/{id}", content);
        return response.IsSuccessStatusCode;
    }
}

internal class ItemsResponse
{
    public List<Item> Items { get; set; } = [];
}

internal class CategoriesResponse
{
    public List<Category> Categories { get; set; } = [];
}