using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

/// <summary>
/// Business logic wrapper around rental API calls. Adds date validation before
/// forwarding to <see cref="IApiService"/>.
/// </summary>
/// <remarks>
/// This service lives in RentalApp.Database rather than in the MAUI project so that
/// RentalServiceTests can unit-test the validation logic. The test project can't reference
/// the MAUI project (it targets net10.0-android and would pull in Android dependencies).
/// Moving it here was the only way to test without a full MAUI runtime.
/// </remarks>
public class RentalService : IRentalService
{
    private readonly IApiService _apiService;

    /// <summary>Creates a new RentalService with the given API service.</summary>
    public RentalService(IApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Validates dates before creating the rental. Returns an error immediately if:
    /// - startDate is in the past
    /// - endDate is not after startDate
    /// Otherwise delegates to the API.
    /// </summary>
    public async Task<(bool Success, string Error)> RequestRentalAsync(
        int itemId, DateTime startDate, DateTime endDate)
    {
        if (startDate.Date < DateTime.Today)
            return (false, "Start date cannot be in the past.");

        if (endDate.Date <= startDate.Date)
            return (false, "End date must be after start date.");

        return await _apiService.CreateRentalAsync(itemId, startDate, endDate);
    }

    /// <summary>Delegates directly to the API — no extra logic needed here.</summary>
    public async Task<List<Rental>> GetIncomingRentalsAsync() =>
        await _apiService.GetIncomingRentalsAsync();

    /// <summary>Delegates directly to the API — no extra logic needed here.</summary>
    public async Task<List<Rental>> GetOutgoingRentalsAsync() =>
        await _apiService.GetOutgoingRentalsAsync();

    /// <summary>
    /// Validates the status string before calling the API. The API would reject invalid
    /// statuses anyway, but failing fast here keeps the error message consistent and
    /// makes it easy to test in RentalServiceTests without hitting the network.
    /// </summary>
    public async Task<(bool Success, string Error)> UpdateRentalStatusAsync(
        int rentalId, string newStatus)
    {
        // Status values are plain strings here, in RentalDisplayItem, and on the API.
        // Should've been a central enum or constants class. A typo or drift between
        // files wouldn't be caught at compile time.
        var validStatuses = new[] { "Approved", "Rejected", "Returned", "Completed" };
        if (!validStatuses.Contains(newStatus))
            return (false, "Invalid status.");

        return await _apiService.UpdateRentalStatusAsync(rentalId, newStatus);
    }
}
