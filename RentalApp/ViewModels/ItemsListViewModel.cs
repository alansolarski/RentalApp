using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class ItemsListViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ItemsListViewModel(IApiService apiService)
    {
        _apiService = apiService;
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
}