using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

public class ReviewService : IReviewService
{
    private readonly IApiService _apiService;

    public ReviewService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId)
        => _apiService.GetItemReviewsAsync(itemId);

    public async Task<(bool Success, string Error)> SubmitReviewAsync(int rentalId, int rating, string? comment)
    {
        try
        {
            await _apiService.SubmitReviewAsync(rentalId, rating, comment);
            return (true, string.Empty);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return (false, "You have already reviewed this rental.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return (false, "Only the borrower can review a completed rental.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}