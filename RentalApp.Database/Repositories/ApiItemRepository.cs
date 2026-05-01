using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.Database.Data.Repositories;

/// <summary>
/// API-backed implementation of <see cref="IItemRepository"/>. Delegates to
/// <see cref="IApiService"/> so the live app path goes through the repository
/// abstraction without touching the local database.
/// </summary>
/// <remarks>
/// Only <see cref="GetAllAsync"/> is needed for the live app path (ItemsListViewModel).
/// The remaining methods throw <see cref="NotImplementedException"/> because item creation,
/// update, and delete go through IApiService directly in their respective ViewModels and
/// don't need the repository abstraction yet.
/// </remarks>
public class ApiItemRepository : IItemRepository
{
    private readonly IApiService _apiService;

    public ApiItemRepository(IApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>Returns all items from GET /items.</summary>
    public Task<IEnumerable<Item>> GetAllAsync() => _apiService.GetItemsAsync();

    public Task<Item?> GetByIdAsync(int id) =>
        throw new NotImplementedException("Use IApiService.GetItemByIdAsync for single-item lookup.");

    public Task<Item> CreateAsync(Item item) =>
        throw new NotImplementedException("Use IApiService.CreateItemAsync for item creation.");

    public Task<Item> UpdateAsync(Item item) =>
        throw new NotImplementedException("Use IApiService.UpdateItemAsync for item updates.");

    public Task DeleteAsync(int id) =>
        throw new NotImplementedException("No delete endpoint exists in the API.");
}
