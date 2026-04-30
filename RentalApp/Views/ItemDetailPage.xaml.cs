using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Item Detail page. Receives the item ID via Shell navigation query.</summary>
/// <remarks>
/// The QueryProperty is on the page rather than the ViewModel because ItemDetailViewModel
/// uses [ObservableProperty] from the MVVM Toolkit, and adding [QueryProperty] directly to
/// the ViewModel didn't play nicely with the Shell navigation pipeline in testing.
/// The setter fires LoadItemCommand immediately so the item loads as soon as the ID arrives.
/// </remarks>
[QueryProperty(nameof(ItemId), "id")]
public partial class ItemDetailPage : ContentPage
{
    private readonly ItemDetailViewModel _viewModel;

    /// <summary>Shell sets this from the "id" query parameter and immediately triggers a load.</summary>
    public int ItemId
    {
        set => _viewModel.LoadItemCommand.Execute(value);
    }

    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
}
