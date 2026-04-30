using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

/// <summary>
/// Business logic layer for rental operations. Lives here in RentalApp.Database so that
/// RentalServiceTests can reference and test it without pulling in MAUI targets.
/// </summary>
/// <remarks>
/// RentalService adds date validation on top of raw API calls. The interface lives in the
/// shared project so the test project can mock or inject it without needing the MAUI runtime.
/// </remarks>
public interface IRentalService
{
    /// <summary>
    /// Validates the date range and then requests a rental via the API.
    /// </summary>
    /// <param name="itemId">The item to rent.</param>
    /// <param name="startDate">Must be today or in the future.</param>
    /// <param name="endDate">Must be after startDate.</param>
    /// <returns>Success flag and an error message if validation or the API call failed.</returns>
    Task<(bool Success, string Error)> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);

    /// <summary>Returns rentals where the current user is the owner.</summary>
    Task<List<Rental>> GetIncomingRentalsAsync();

    /// <summary>Returns rentals where the current user is the borrower.</summary>
    Task<List<Rental>> GetOutgoingRentalsAsync();

    /// <summary>
    /// Updates a rental's status. Validates the new status before hitting the API.
    /// </summary>
    /// <param name="rentalId">The rental to update.</param>
    /// <param name="newStatus">Must be one of: Approved, Rejected, Returned, Completed.</param>
    Task<(bool Success, string Error)> UpdateRentalStatusAsync(int rentalId, string newStatus);
}
