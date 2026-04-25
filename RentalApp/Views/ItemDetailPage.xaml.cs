using RentalApp.ViewModels;

namespace RentalApp.Views;

[QueryProperty(nameof(ItemId), "id")]
public partial class ItemDetailPage : ContentPage
{
    private readonly ItemDetailViewModel _viewModel;

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