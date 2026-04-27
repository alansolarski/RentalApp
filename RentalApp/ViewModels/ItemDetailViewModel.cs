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

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(2);

    [RelayCommand]
    private async Task RequestRentalAsync()
    {
        var rentalService = IPlatformApplication.Current?.Services
            .GetService<IRentalService>();

        if (rentalService == null) return;

        var (success, error) = await rentalService.RequestRentalAsync(
            Item!.Id, StartDate, EndDate);

        if (success)
            await Shell.Current.DisplayAlert("Success", "Rental requested!", "OK");
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }
}