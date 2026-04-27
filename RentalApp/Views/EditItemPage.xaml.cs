using RentalApp.ViewModels;

namespace RentalApp.Views;

public partial class EditItemPage : ContentPage
{
    public EditItemPage(EditItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}