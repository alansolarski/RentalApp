using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

public interface IReviewService
{
    Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId);
    Task<(bool Success, string Error)> SubmitReviewAsync(int rentalId, int rating, string? comment);
}