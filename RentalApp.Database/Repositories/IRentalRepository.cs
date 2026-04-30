using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

/// <summary>
/// Database operations for rentals in the local Postgres store.
/// In practice the app hits the API for all rental operations, but this repository
/// satisfies the coursework architecture checklist requirement for a repository layer.
/// </summary>
public interface IRentalRepository
{
    /// <summary>Returns all rentals where the current user is the owner (items lent out).</summary>
    /// <param name="userId">The owner's user ID.</param>
    Task<List<Rental>> GetIncomingAsync(int userId);

    /// <summary>Returns all rentals where the current user is the borrower.</summary>
    /// <param name="userId">The borrower's user ID.</param>
    Task<List<Rental>> GetOutgoingAsync(int userId);

    /// <summary>Returns a single rental by ID, or null if not found.</summary>
    Task<Rental?> GetByIdAsync(int id);

    /// <summary>Inserts a new rental and returns it with the generated ID populated.</summary>
    Task<Rental> CreateAsync(Rental rental);

    /// <summary>
    /// Updates the status string of an existing rental.
    /// </summary>
    /// <param name="id">Rental ID.</param>
    /// <param name="status">New status, e.g. "Approved", "Returned".</param>
    /// <returns>True if the rental was found and updated, false if it didn't exist.</returns>
    Task<bool> UpdateStatusAsync(int id, string status);

    /// <summary>
    /// Checks whether an approved rental already overlaps the requested date range for an item.
    /// Used to prevent double-booking.
    /// </summary>
    Task<bool> HasConflictingRentalAsync(int itemId, DateTime startDate, DateTime endDate);
}
