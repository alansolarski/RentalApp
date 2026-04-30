using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRentalRepository"/> backed by the local Postgres database.
/// </summary>
/// <remarks>
/// The app's live rental flow goes through the API (via ApiService), not this repository.
/// This exists to satisfy the architecture requirement and give the conflict check a testable,
/// database-backed implementation.
/// </remarks>
public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    /// <summary>Creates a new repository bound to the given database context.</summary>
    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all rentals owned by the user, newest first.</summary>
    public async Task<List<Rental>> GetIncomingAsync(int userId) =>
        await _context.Rentals
            .Where(r => r.OwnerId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    /// <summary>Returns all rentals borrowed by the user, newest first.</summary>
    public async Task<List<Rental>> GetOutgoingAsync(int userId) =>
        await _context.Rentals
            .Where(r => r.BorrowerId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    /// <summary>Returns the rental with the given ID, or null.</summary>
    public async Task<Rental?> GetByIdAsync(int id) =>
        await _context.Rentals.FindAsync(id);

    /// <summary>Inserts a new rental and returns it with the database-assigned ID.</summary>
    public async Task<Rental> CreateAsync(Rental rental)
    {
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();
        return rental;
    }

    /// <summary>Updates the status of an existing rental. Returns false if the rental wasn't found.</summary>
    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var rental = await _context.Rentals.FindAsync(id);
        if (rental == null) return false;
        rental.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks for an approved rental that overlaps the requested window.
    /// Uses an open-interval check: startDate &lt; existingEnd AND endDate &gt; existingStart.
    /// Only "Approved" rentals count — "Requested" or "Rejected" ones don't block dates.
    /// </summary>
    public async Task<bool> HasConflictingRentalAsync(int itemId, DateTime startDate, DateTime endDate) =>
        await _context.Rentals
            .AnyAsync(r => r.ItemId == itemId
                && r.Status == "Approved"
                && r.StartDate < endDate
                && r.EndDate > startDate);
}
