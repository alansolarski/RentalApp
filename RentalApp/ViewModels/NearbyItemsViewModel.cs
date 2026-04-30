using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Nearby Items page. Gets the device's current location and calls
/// GET /items/nearby to find rental items within the selected radius.
/// </summary>
public partial class NearbyItemsViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ILocationService _locationService;

    [ObservableProperty]
    private ObservableCollection<NearbyItem> _nearbyItems = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Tap 'Find Near Me' to search";

    /// <summary>Search radius in kilometres — bound to a Slider in the XAML. Defaults to 5km.</summary>
    [ObservableProperty]
    private double _searchRadius = 5;

    public NearbyItemsViewModel(IApiService apiService, ILocationService locationService)
    {
        _apiService = apiService;
        _locationService = locationService;
    }

    /// <summary>
    /// Gets the current location from the device and searches for nearby items.
    /// If location permission is denied or unavailable, shows a message instead of throwing.
    /// </summary>
    [RelayCommand]
    private async Task FindNearbyAsync()
    {
        IsLoading = true;
        StatusMessage = "Getting your location...";
        NearbyItems.Clear();

        try
        {
            var location = await _locationService.GetCurrentLocationAsync();

            if (location == null)
            {
                StatusMessage = "Could not get your location. Please enable location permissions.";
                return;
            }

            StatusMessage = $"Searching within {SearchRadius}km...";

            var items = await _apiService.GetNearbyItemsAsync(
                location.Value.Latitude,
                location.Value.Longitude,
                SearchRadius);

            if (items.Count == 0)
            {
                StatusMessage = $"No items found within {SearchRadius}km.";
                return;
            }

            foreach (var item in items)
                NearbyItems.Add(item);

            StatusMessage = $"Found {items.Count} item(s) near you";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Navigates to the detail page for the tapped nearby item.</summary>
    [RelayCommand]
    private async Task NavigateToDetailAsync(NearbyItem item)
    {
        await Shell.Current.GoToAsync($"ItemDetailPage?id={item.Id}");
    }
}
