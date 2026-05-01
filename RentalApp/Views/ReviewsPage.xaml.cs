using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Reviews page. Handles the three query parameters and the star rating picker.</summary>
/// <remarks>
/// This page is navigated to from two places with different parameter sets:
///   - ItemDetailPage: sends only "itemId" (view mode — user just browses reviews)
///   - RentalsPage LeaveReview: sends "itemId", "rentalId", and "canReview=true" (review form shown)
///
/// The three QueryProperty attributes map each Shell query parameter to a setter here,
/// which then push the value straight into ReviewsViewModel. The page class is the
/// recipient because Shell's [QueryProperty] requires the attribute to be on a ContentPage.
/// ReviewsViewModel is transient — a fresh instance is created on each navigation, and these
/// setters re-populate it immediately, so no state is lost.
/// </remarks>
[QueryProperty(nameof(ItemId), "itemId")]
[QueryProperty(nameof(RentalId), "rentalId")]
[QueryProperty(nameof(CanReview), "canReview")]
public partial class ReviewsPage : ContentPage
{
    private readonly ReviewsViewModel _viewModel;

    /// <summary>Triggers a review load immediately when the ID arrives from the query string.</summary>
    public int ItemId
    {
        set
        {
            System.Diagnostics.Debug.WriteLine($"### ReviewsPage ItemId setter called with: {value}");
            _ = _viewModel.LoadReviewsAsync(value);
        }
    }

    /// <summary>Passed through from RentalsPage so the ViewModel knows which rental to submit against.</summary>
    public int RentalId
    {
        set => _viewModel.RentalId = value;
    }

    /// <summary>Set to true when navigating from RentalsPage to show the review form.</summary>
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

    /// <summary>Handles the star button clicks and updates the selected rating on the ViewModel.</summary>
    /// <remarks>
    /// MAUI doesn't have a built-in star rating control, so I used a row of Buttons labelled
    /// "★1", "★2", ... "★5". This handler strips the star character, parses the number,
    /// and sets SelectedRating on the ViewModel. It's a workaround but it works reliably.
    /// </remarks>
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
