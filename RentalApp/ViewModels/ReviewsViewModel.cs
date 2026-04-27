using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class ReviewsViewModel : ObservableObject
{
    private readonly IReviewService _reviewService;

    public ReviewsViewModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [ObservableProperty] private ObservableCollection<Review> _reviews = new();
    [ObservableProperty] private int _itemId;
    [ObservableProperty] private int _rentalId;
    [ObservableProperty] private bool _canReview;
    [ObservableProperty] private int _selectedRating = 5;
    [ObservableProperty] private string _comment = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isSubmitting;

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

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (RentalId == 0) return;

        IsSubmitting = true;
        StatusMessage = string.Empty;

        try
        {
            var (success, error) = await _reviewService.SubmitReviewAsync(RentalId, SelectedRating, Comment);
            if (success)
            {
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