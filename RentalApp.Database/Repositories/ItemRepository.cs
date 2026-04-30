using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IItemRepository"/> backed by the local Postgres database.
/// </summary>
/// <remarks>
/// All queries include the Owner navigation property so callers don't need separate user lookups.
/// The main rental flow uses <see cref="RentalApp.Database.Services.IApiService"/> instead,
/// but this repo is exercised by <c>ItemRepositoryTests</c> to verify the local DB layer.
/// </remarks>
public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    /// <summary>Creates a new repository bound to the given database context.</summary>
    /// <param name="context">The EF Core context, injected by DI.</param>
    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all items with their Owner included.</summary>
    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _context.Items
            .Include(i => i.Owner)
            .ToListAsync();
    }

    /// <summary>Returns the item with the given ID and its Owner, or null if not found.</summary>
    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    /// <summary>Inserts the item and returns it with the database-assigned ID.</summary>
    public async Task<Item> CreateAsync(Item item)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <summary>Saves changes to an existing tracked item.</summary>
    public async Task<Item> UpdateAsync(Item item)
    {
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <summary>Deletes the item with the given ID. Silently does nothing if the ID doesn't exist.</summary>
    public async Task DeleteAsync(int id)
    {
        var item = await GetByIdAsync(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
