using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Rentals page.</summary>
public partial class RentalsPage : ContentPage
{
    public RentalsPage(RentalsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload on every appear so the list is fresh after actions like approve/reject
        // that navigate away and come back, or after leaving a review on ReviewsPage.
        ((RentalsViewModel)BindingContext).LoadRentalsCommand.Execute(null);
    }
}
