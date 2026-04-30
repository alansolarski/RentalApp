using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>
/// Code-behind for the Edit Item page. The item is loaded via QueryProperty on
/// EditItemViewModel directly, so there's nothing extra to do in OnAppearing.
/// </summary>
public partial class EditItemPage : ContentPage
{
    public EditItemPage(EditItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
