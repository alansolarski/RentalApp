using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Profile page.</summary>
public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Call the method directly rather than via a command because LoadProfileAsync
        // is async and command.Execute() doesn't await — calling it directly avoids
        // a fire-and-forget pattern that would swallow exceptions silently.
        await _viewModel.LoadProfileAsync();
    }
}
