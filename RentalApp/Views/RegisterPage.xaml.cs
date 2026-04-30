using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Register page. All logic is in RegisterViewModel.</summary>
public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
