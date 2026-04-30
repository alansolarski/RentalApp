using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

/// <summary>
/// Wraps review API calls and translates HTTP error codes into human-readable messages.
/// </summary>
/// <remarks>
/// Lives in RentalApp.Database (instead of the MAUI project) for the same reason as
/// RentalService — the test project needs to reference it without pulling in Android targets.
/// </remarks>
public class ReviewService : IReviewService
{
    private readonly IApiService _apiService;

    /// <summary>Creates a new ReviewService with the given API service.</summary>
    public ReviewService(IApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>Fetches reviews for an item. Delegates directly to ApiService — no extra logic.</summary>
    public Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId)
        => _apiService.GetItemReviewsAsync(itemId);

    /// <summary>
    /// Submits a review and translates specific HTTP errors into clear user messages.
    /// The API returns 409 Conflict if you've already reviewed this rental, and 403 Forbidden
    /// if you're the owner trying to review your own item. Both are mapped here so the UI
    /// doesn't have to know about HTTP status codes.
    /// </summary>
    public async Task<(bool Success, string Error)> SubmitReviewAsync(int rentalId, int rating, string? comment)
    {
        try
        {
            await _apiService.SubmitReviewAsync(rentalId, rating, comment);
            return (true, string.Empty);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            // The API enforces one review per rental — duplicate attempts get a 409.
            return (false, "You have already reviewed this rental.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            // The API only lets the borrower review, not the owner.
            return (false, "Only the borrower can review a completed rental.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
