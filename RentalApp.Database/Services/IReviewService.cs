using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

/// <summary>
/// Business logic layer for review operations. Lives here in RentalApp.Database so that
/// ReviewServiceTests can reference and test it without pulling in MAUI targets.
/// </summary>
public interface IReviewService
{
    /// <summary>Returns all reviews for the given item.</summary>
    Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId);

    /// <summary>
    /// Submits a review via the API and wraps HTTP errors into meaningful messages.
    /// </summary>
    /// <param name="rentalId">The rental being reviewed. One review allowed per rental.</param>
    /// <param name="rating">Star rating 1–5.</param>
    /// <param name="comment">Optional free-text comment.</param>
    /// <returns>Success flag and an error message. 409 Conflict returns a "already reviewed" message.</returns>
    Task<(bool Success, string Error)> SubmitReviewAsync(int rentalId, int rating, string? comment);
}
