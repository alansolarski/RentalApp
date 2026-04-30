using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

/// <summary>
/// CRUD operations for <see cref="Item"/> in the local Postgres database.
/// Used by the admin/local path — the main rental flow talks to the API through
/// <see cref="RentalApp.Database.Services.IApiService"/> instead.
/// </summary>
public interface IItemRepository
{
    /// <summary>Returns all items with their Owner navigation property loaded.</summary>
    Task<IEnumerable<Item>> GetAllAsync();

    /// <summary>Returns a single item by ID, or null if not found.</summary>
    /// <param name="id">The item's primary key.</param>
    Task<Item?> GetByIdAsync(int id);

    /// <summary>Inserts a new item and returns it with the generated ID populated.</summary>
    /// <param name="item">The item to create.</param>
    Task<Item> CreateAsync(Item item);

    /// <summary>Updates an existing item and returns the saved entity.</summary>
    /// <param name="item">The item with updated values.</param>
    Task<Item> UpdateAsync(Item item);

    /// <summary>Permanently deletes an item. Does nothing if the ID doesn't exist.</summary>
    /// <param name="id">The ID of the item to delete.</param>
    Task DeleteAsync(int id);
}
