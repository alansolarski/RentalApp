using RentalApp.ViewModels;

namespace RentalApp.Views;

/// <summary>Code-behind for the Main (dashboard) page. All logic is in MainViewModel.</summary>
public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
