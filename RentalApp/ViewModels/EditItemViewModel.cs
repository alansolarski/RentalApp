using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Edit Item page. Receives the item ID via Shell query parameter,
/// loads the current item values, and saves changes via PUT /items/{id}.
/// </summary>
[QueryProperty(nameof(ItemId), "id")]
public partial class EditItemViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private int _itemId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _dailyRate = string.Empty;

    [ObservableProperty]
    private bool _isAvailable;

    [ObservableProperty]
    private bool _isLoading;

    public EditItemViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Fires when the ItemId query parameter is set by Shell navigation.
    /// Triggers the item load so the form is populated before the user sees it.
    /// </summary>
    partial void OnItemIdChanged(int value)
    {
        if (value > 0)
            LoadItemCommand.Execute(null);
    }

    /// <summary>Fetches the current item values from GET /items/{id} and populates the form.</summary>
    [RelayCommand]
    private async Task LoadItemAsync()
    {
        IsLoading = true;
        try
        {
            var item = await _apiService.GetItemByIdAsync(ItemId);
            if (item != null)
            {
                Title = item.Title;
                Description = item.Description ?? string.Empty;
                DailyRate = item.DailyRate.ToString("F2");
                IsAvailable = item.IsAvailable;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Validates and saves the edited item via PUT /items/{id}.
    /// Shows a Shell DisplayAlert rather than an inline error because this page
    /// doesn't have an error label in the XAML.
    /// </summary>
    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Shell.Current.DisplayAlert("Validation", "Title is required.", "OK");
            return;
        }

        if (!decimal.TryParse(DailyRate, out var rate) || rate <= 0)
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter a valid daily rate.", "OK");
            return;
        }

        IsLoading = true;
        try
        {
            var request = new UpdateItemRequest
            {
                Title = Title,
                Description = Description,
                DailyRate = rate,
                IsAvailable = IsAvailable
            };

            var success = await _apiService.UpdateItemAsync(ItemId, request);

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Item updated.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to update item.", "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
