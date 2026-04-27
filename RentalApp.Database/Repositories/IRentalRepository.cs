using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public interface IRentalRepository
{
    Task<List<Rental>> GetIncomingAsync(int userId);
    Task<List<Rental>> GetOutgoingAsync(int userId);
    Task<Rental?> GetByIdAsync(int id);
    Task<Rental> CreateAsync(Rental rental);
    Task<bool> UpdateStatusAsync(int id, string status);
    Task<bool> HasConflictingRentalAsync(int itemId, DateTime startDate, DateTime endDate);
}