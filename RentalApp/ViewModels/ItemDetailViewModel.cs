using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class ItemDetailViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    [ObservableProperty] private Item? item;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;

    public ItemDetailViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task LoadItemAsync(int id)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            Item = await _apiService.GetItemByIdAsync(id);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load item: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToEditAsync()
    {
        await Shell.Current.GoToAsync($"EditItemPage?id={Item?.Id}");
    }
}