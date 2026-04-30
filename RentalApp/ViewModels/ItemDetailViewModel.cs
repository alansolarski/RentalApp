using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;
using RentalApp.Views;

namespace RentalApp.ViewModels;

public partial class ItemDetailViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly TokenStore _tokenStore;

    [ObservableProperty] private Item? item;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(2);

    public bool IsOwner => Item?.OwnerId == _tokenStore.UserId;

    public ItemDetailViewModel(
        IApiService apiService,
        INavigationService navigationService,
        TokenStore tokenStore)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _tokenStore = tokenStore;
    }

    partial void OnItemChanged(Item? value)
    {
        OnPropertyChanged(nameof(IsOwner));
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

    [RelayCommand]
    private async Task NavigateToReviewsAsync()
    {
        await Shell.Current.GoToAsync(
            $"{nameof(ReviewsPage)}?itemId={Item.Id}");
    }
}