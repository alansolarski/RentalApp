using RentalApp.ViewModels;

namespace RentalApp.Views;

[QueryProperty(nameof(ItemId), "itemId")]
[QueryProperty(nameof(RentalId), "rentalId")]
[QueryProperty(nameof(CanReview), "canReview")]
public partial class ReviewsPage : ContentPage
{
    private readonly ReviewsViewModel _viewModel;

    public int ItemId
    {
        set
        {
            System.Diagnostics.Debug.WriteLine($"### ReviewsPage ItemId setter called with: {value}");
            _ = _viewModel.LoadReviewsAsync(value);
        }
    }

    public int RentalId
    {
        set => _viewModel.RentalId = value;
    }

    public bool CanReview
    {
        set => _viewModel.CanReview = value;
    }

    public ReviewsPage(ReviewsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private void OnStarClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && BindingContext is ReviewsViewModel vm)
        {
            var text = btn.Text.Replace("★", "").Trim();
            if (int.TryParse(text, out int rating))
                vm.SelectedRating = rating;
        }
    }
}