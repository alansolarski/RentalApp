using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Reviews page. Handles loading all reviews for an item and optionally
/// showing the "leave a review" form for a specific rental.
/// </summary>
/// <remarks>
/// Registered as a singleton in MauiProgram because ReviewsPage can be reached from two places:
/// ItemDetailPage (just viewing reviews) and RentalsPage (leaving a review after a completed rental).
/// The singleton means query parameters from the second navigation path don't get lost.
/// </remarks>
public partial class ReviewsViewModel : ObservableObject
{
    private readonly IReviewService _reviewService;

    public ReviewsViewModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [ObservableProperty] private ObservableCollection<Review> _reviews = new();

    /// <summary>The item whose reviews are shown. Set by the ItemId query parameter on ReviewsPage.</summary>
    [ObservableProperty] private int _itemId;

    /// <summary>The specific rental being reviewed. Set when navigating from RentalsPage LeaveReview.</summary>
    [ObservableProperty] private int _rentalId;

    /// <summary>True when the review form should be shown. Set to false after a successful submission.</summary>
    [ObservableProperty] private bool _canReview;

    [ObservableProperty] private int _selectedRating = 5;
    [ObservableProperty] private string _comment = string.Empty;

    /// <summary>Status message shown below the review form — either a success or error message.</summary>
    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private bool _isSubmitting;

    /// <summary>
    /// Loads all reviews for the given item from GET /items/{id}/reviews.
    /// Skips the load if itemId is 0 to avoid unnecessary API calls.
    /// </summary>
    [RelayCommand]
    public async Task LoadReviewsAsync(int itemId)
    {
        if (itemId == 0) return;
        ItemId = itemId;
        try
        {
            var reviews = await _reviewService.GetItemReviewsAsync(itemId);
            Reviews = new ObservableCollection<Review>(reviews);
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load reviews: {ex.Message}";
        }
    }

    /// <summary>
    /// Submits the review for the current rental. On success, hides the form and reloads reviews.
    /// On failure, displays the error message from ReviewService (which translates HTTP status codes).
    /// </summary>
    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        // Guard against navigating here without a rental context.
        if (RentalId == 0) return;

        IsSubmitting = true;
        StatusMessage = string.Empty;

        try
        {
            var (success, error) = await _reviewService.SubmitReviewAsync(RentalId, SelectedRating, Comment);
            if (success)
            {
                // Hide the form — you can only review once per rental.
                CanReview = false;
                Comment = string.Empty;
                StatusMessage = "Review submitted!";
                await LoadReviewsAsync(ItemId);
            }
            else
            {
                StatusMessage = error;
            }
        }
        finally
        {
            IsSubmitting = false;
        }
    }
}
