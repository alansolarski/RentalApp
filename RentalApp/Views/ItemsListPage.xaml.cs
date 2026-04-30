using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Items List page.</summary>
public partial class ItemsListPage : ContentPage
{
    public ItemsListPage(ItemsListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload on every appear so the list reflects any items created or edited
        // while the user was elsewhere (e.g. returned from CreateItemPage).
        ((ItemsListViewModel)BindingContext).LoadItemsCommand.Execute(null);
    }
}
