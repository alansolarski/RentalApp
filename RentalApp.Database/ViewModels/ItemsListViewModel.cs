using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.Database.ViewModels;

/// <summary>
/// ViewModel for the items list screen. This copy lives in RentalApp.Database specifically
/// so that ItemsListViewModelTests can reference it without pulling in the MAUI project.
/// </summary>
/// <remarks>
/// There are two copies of this ViewModel:
/// - <c>RentalApp.Database/ViewModels/ItemsListViewModel.cs</c> — this file, used by tests.
/// - <c>RentalApp/ViewModels/ItemsListViewModel.cs</c> — used by the MAUI app at runtime.
///
/// They must be kept in sync manually. A cleaner solution would be a separate RentalApp.Core
/// class library, but that was out of scope for the coursework timeline. The MAUI project can't
/// be referenced by the test project because it targets net10.0-android and would bring in
/// Android platform dependencies.
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

    /// <summary>Creates the ViewModel with its required dependencies.</summary>
    /// <param name="itemRepository">Used to fetch items — abstracts the API behind a repository.</param>
    /// <param name="navigationService">Used for page navigation commands.</param>
    public ItemsListViewModel(IItemRepository itemRepository, INavigationService navigationService)
    {
        _itemRepository = itemRepository;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Loads all items via the repository and populates the Items collection.
    /// Sets ErrorMessage if the call fails, clears it on success.
    /// IsLoading is guaranteed to be reset in the finally block.
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

    /// <summary>Navigates to the create item page.</summary>
    [RelayCommand]
    public async Task NavigateToCreateAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }

    /// <summary>Navigates to the detail page for a specific item.</summary>
    /// <param name="id">The item's ID, passed as a query parameter.</param>
    [RelayCommand]
    public async Task NavigateToDetailAsync(int id)
    {
        await _navigationService.NavigateToAsync($"ItemDetailPage?id={id}");
    }

    /// <summary>Navigates to the nearby items map search page.</summary>
    [RelayCommand]
    private async Task NavigateToNearbyAsync()
    {
        await _navigationService.NavigateToAsync("NearbyItemsPage");
    }

    /// <summary>Navigates to the rentals management page.</summary>
    [RelayCommand]
    private async Task NavigateToRentalsAsync()
    {
        await _navigationService.NavigateToAsync("RentalsPage");
    }

    /// <summary>Navigates to the user profile page.</summary>
    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        await _navigationService.NavigateToAsync("ProfilePage");
    }
}
