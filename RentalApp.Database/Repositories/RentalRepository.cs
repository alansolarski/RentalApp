using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Models;

namespace RentalApp.Database.Data.Repositories;

public class RentalRepository : IRentalRepository
{
    private readonly AppDbContext _context;

    public RentalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rental>> GetIncomingAsync(int userId) =>
        await _context.Rentals
            .Where(r => r.OwnerId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public async Task<List<Rental>> GetOutgoingAsync(int userId) =>
        await _context.Rentals
            .Where(r => r.BorrowerId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public async Task<Rental?> GetByIdAsync(int id) =>
        await _context.Rentals.FindAsync(id);

    public async Task<Rental> CreateAsync(Rental rental)
    {
        _context.Rentals.Add(rental);
        await _context.SaveChangesAsync();
        return rental;
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var rental = await _context.Rentals.FindAsync(id);
        if (rental == null) return false;
        rental.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasConflictingRentalAsync(int itemId, DateTime startDate, DateTime endDate) =>
        await _context.Rentals
            .AnyAsync(r => r.ItemId == itemId
                && r.Status == "Approved"
                && r.StartDate < endDate
                && r.EndDate > startDate);
}