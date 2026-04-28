using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

public interface IRentalService
{
    Task<(bool Success, string Error)> RequestRentalAsync(int itemId, DateTime startDate, DateTime endDate);
    Task<List<Rental>> GetIncomingRentalsAsync();
    Task<List<Rental>> GetOutgoingRentalsAsync();
    Task<(bool Success, string Error)> UpdateRentalStatusAsync(int rentalId, string newStatus);
}