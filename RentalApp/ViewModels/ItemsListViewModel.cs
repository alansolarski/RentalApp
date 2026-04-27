using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class ItemsListViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ItemsListViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task LoadItemsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _apiService.GetItemsAsync();
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

    [RelayCommand]
    public async Task NavigateToCreateAsync()
    {
        await _navigationService.NavigateToAsync("CreateItemPage");
    }

    [RelayCommand]
    public async Task NavigateToDetailAsync(int id)
    {
        await _navigationService.NavigateToAsync($"ItemDetailPage?id={id}");
    }

    [RelayCommand]
    private async Task NavigateToNearbyAsync()
    {
        await Shell.Current.GoToAsync("NearbyItemsPage");
    }

    [RelayCommand]
    private async Task NavigateToRentalsAsync()
    {
        await Shell.Current.GoToAsync("RentalsPage");
    }
}