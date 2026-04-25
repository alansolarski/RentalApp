using RentalApp.ViewModels;
using RentalApp.Views;

namespace RentalApp;

public partial class AppShell : Shell
{
	public AppShell(AppShellViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;

		Routing.RegisterRoute(nameof(ItemsListPage), typeof(ItemsListPage));
	}
}