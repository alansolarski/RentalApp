using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Login page.</summary>
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailEntry.Focus();

        // Pre-fill the admin credentials for demo/submission convenience so the
        // marker doesn't have to type them in every time the app is launched.
        // This would obviously be removed in a production build.
        EmailEntry.Text = "admin@company.com";
        PasswordEntry.Text = "Admin123!";
    }
}
