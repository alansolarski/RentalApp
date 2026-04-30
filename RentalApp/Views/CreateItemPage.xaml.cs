using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Create Item page.</summary>
public partial class CreateItemPage : ContentPage
{
    public CreateItemPage(CreateItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Load categories on every appear so the picker stays in sync if
        // a new category was added via another route while this page was in the stack.
        ((CreateItemViewModel)BindingContext).LoadCategoriesCommand.Execute(null);
    }
}
