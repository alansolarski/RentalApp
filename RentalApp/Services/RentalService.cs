using RentalApp.Database.Models;

namespace RentalApp.Services;

public class RentalService : IRentalService
{
    private readonly IApiService _apiService;

    public RentalService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<(bool Success, string Error)> RequestRentalAsync(
        int itemId, DateTime startDate, DateTime endDate)
    {
        if (startDate.Date < DateTime.Today)
            return (false, "Start date cannot be in the past.");

        if (endDate.Date <= startDate.Date)
            return (false, "End date must be after start date.");

        return await _apiService.CreateRentalAsync(itemId, startDate, endDate);
    }

    public async Task<List<Rental>> GetIncomingRentalsAsync() =>
        await _apiService.GetIncomingRentalsAsync();

    public async Task<List<Rental>> GetOutgoingRentalsAsync() =>
        await _apiService.GetOutgoingRentalsAsync();

    public async Task<(bool Success, string Error)> UpdateRentalStatusAsync(
        int rentalId, string newStatus)
    {
        var validStatuses = new[] { "Approved", "Rejected", "Returned", "Completed" };
        if (!validStatuses.Contains(newStatus))
            return (false, "Invalid status.");

        return await _apiService.UpdateRentalStatusAsync(rentalId, newStatus);
    }
}