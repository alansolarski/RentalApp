using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.ViewModels;

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

    partial void OnItemIdChanged(int value)
    {
        if (value > 0)
            LoadItemCommand.Execute(null);
    }

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