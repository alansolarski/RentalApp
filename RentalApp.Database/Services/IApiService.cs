using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

public interface IApiService
{
    Task<IEnumerable<Item>> GetItemsAsync();
    Task<Item?> GetItemByIdAsync(int id);
    Task<Item?> CreateItemAsync(Item item);
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<bool> UpdateItemAsync(int id, UpdateItemRequest request);
    Task<List<NearbyItem>> GetNearbyItemsAsync(double lat, double lon, double radiusKm = 5);
    Task<(bool Success, string Error)> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate);
    Task<List<Rental>> GetIncomingRentalsAsync();
    Task<List<Rental>> GetOutgoingRentalsAsync();
    Task<(bool Success, string Error)> UpdateRentalStatusAsync(int rentalId, string status);
    Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId);
    Task<Review> SubmitReviewAsync(int rentalId, int rating, string? comment);
}