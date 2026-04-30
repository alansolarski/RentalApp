using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>
/// Code-behind for the User Detail page. Handles both viewing/editing an existing user
/// and creating a new one — the ViewModel distinguishes the two modes by checking whether
/// userId is 0 (new) or a real ID (edit).
/// </summary>
public partial class UserDetailPage : ContentPage
{
    public UserDetailPage(UserDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
