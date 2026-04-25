using RentalApp.Database.Models;

namespace RentalApp.Services;

public interface IApiService
{
    Task<IEnumerable<Item>> GetItemsAsync();
    Task<Item?> GetItemByIdAsync(int id);
    Task<Item?> CreateItemAsync(Item item);
    Task<IEnumerable<Category>> GetCategoriesAsync();
}