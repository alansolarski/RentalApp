using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>
/// Code-behind for the User List page (admin only). UserListViewModel handles
/// the admin guard and redirects non-admins before data loads.
/// </summary>
public partial class UserListPage : ContentPage
{
    public UserListPage(UserListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
