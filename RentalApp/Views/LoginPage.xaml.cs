using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Login page.</summary>
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        HideSoftInputOnTapped = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailEntry.Focus();
    }
}
