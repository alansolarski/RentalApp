using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>
/// Code-behind for the Nearby Items page. Location fetching and the API call
/// are both handled in NearbyItemsViewModel via a command bound in the XAML.
/// </summary>
public partial class NearbyItemsPage : ContentPage
{
    public NearbyItemsPage(NearbyItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
