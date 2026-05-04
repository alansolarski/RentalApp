using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;
using RentalApp.Views;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Item Detail page. Loads item data, controls which action buttons
/// are visible (Edit, Request Rental, View Reviews), and submits rental requests.
/// </summary>
public partial class ItemDetailViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly TokenStore _tokenStore;

    [ObservableProperty] private Item? item;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;

    /// <summary>Default rental start date — tomorrow, so users always start with a valid date.</summary>
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(1);

    /// <summary>Default rental end date — two days from now, giving a one-day rental window.</summary>
    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(2);

    /// <summary>
    /// True if the currently logged-in user owns this item.
    /// Used to show/hide the Edit button — it was previously showing for all users, which
    /// was fixed by comparing Item.OwnerId against TokenStore.UserId.
    /// </summary>
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

    /// <summary>
    /// Re-evaluates IsOwner when the item loads. Without this, IsOwner would be computed
    /// before Item is set and always return false.
    /// </summary>
    partial void OnItemChanged(Item? value)
    {
        OnPropertyChanged(nameof(IsOwner));
    }

    /// <summary>Loads the item from GET /items/{id}. Called from ItemDetailPage when the id query param arrives.</summary>
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

    /// <summary>Navigates to the Edit Item page with the current item's ID.</summary>
    [RelayCommand]
    private async Task NavigateToEditAsync()
    {
        await Shell.Current.GoToAsync($"EditItemPage?id={Item?.Id}");
    }

    // Service Locator here, not constructor injection. Hit a circular DI issue while wiring
    // this up and grabbed RentalService out of the container at call time as a quick fix.
    // Means this can't be swapped in a test, which is why there's no test class for
    // ItemDetailViewModel. Every other ViewModel uses constructor injection.    [RelayCommand]
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

    /// <summary>Navigates to the Reviews page for this item.</summary>
    [RelayCommand]
    private async Task NavigateToReviewsAsync()
    {
        await Shell.Current.GoToAsync(
            $"{nameof(ReviewsPage)}?itemId={Item.Id}");
    }
}
