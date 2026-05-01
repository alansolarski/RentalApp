using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Items List page. Loads all available items and provides navigation
/// commands to other sections of the app.
/// </summary>
/// <remarks>
/// This is one of two copies of this ViewModel. The other lives at
/// RentalApp.Database/ViewModels/ItemsListViewModel.cs and is the one referenced by
/// ItemsListViewModelTests. Both copies must be kept in sync manually. The split exists
/// because the test project can't reference the MAUI project without pulling in Android targets.
/// See the Database copy's class summary for the full explanation.
/// </remarks>
public partial class ItemsListViewModel : ObservableObject
{
    private readonly IItemRepository _itemRepository;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ItemsListViewModel(IItemRepository itemRepository, INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Fetches all items via the repository and replaces the current collection.
    /// Called from ItemsListPage.OnAppearing so the list refreshes on every visit.
    /// </summary>
    [RelayCommand]
    public async Task LoadItemsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _itemRepository.GetAllAsync();
            Items = new ObservableCollection<Item>(items);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load items: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Navigates to the Create Item page.</summary>
    [RelayCommand]
    public async Task NavigateToCreateAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }

    /// <summary>Navigates to the detail page for the tapped item.</summary>
    /// <param name="id">The item's ID, passed from the CollectionView SelectionChanged binding.</param>
    [RelayCommand]
    public async Task NavigateToDetailAsync(int id)
    {
        await _navigationService.NavigateToAsync($"ItemDetailPage?id={id}");
    }

    /// <summary>Navigates to the Nearby Items location search page.</summary>
    [RelayCommand]
    private async Task NavigateToNearbyAsync()
    {
        await _navigationService.NavigateToAsync("NearbyItemsPage");
    }

    /// <summary>Navigates to the Rentals management page.</summary>
    [RelayCommand]
    private async Task NavigateToRentalsAsync()
    {
        await _navigationService.NavigateToAsync("RentalsPage");
    }

    /// <summary>Navigates to the user's Profile page.</summary>
    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        await _navigationService.NavigateToAsync("ProfilePage");
    }
}
