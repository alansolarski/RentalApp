using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;

namespace RentalApp.ViewModels;

public partial class CreateItemViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string dailyRate = string.Empty;
    [ObservableProperty] private string latitude = string.Empty;
    [ObservableProperty] private string longitude = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool hasError;
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private ObservableCollection<Category> categories = [];

    public bool IsNotLoading => !IsLoading;

    public CreateItemViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        var cats = await _apiService.GetCategoriesAsync();
        Categories = new ObservableCollection<Category>(cats);
    }

    [RelayCommand]
    public async Task CreateItemAsync()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(DailyRate))
        {
            ErrorMessage = "Title and daily rate are required.";
            HasError = true;
            return;
        }

        if (!decimal.TryParse(DailyRate, out var rate) || rate <= 0)
        {
            ErrorMessage = "Please enter a valid daily rate.";
            HasError = true;
            return;
        }

        IsLoading = true;
        HasError = false;
        OnPropertyChanged(nameof(IsNotLoading));

        try
        {
            var item = new Item
            {
                Title = Title,
                Description = Description,
                DailyRate = rate,
                CategoryId = SelectedCategory?.Id ?? 1,
                Latitude = double.TryParse(Latitude, out var lat) ? lat : null,
                Longitude = double.TryParse(Longitude, out var lon) ? lon : null
            };

            var result = await _apiService.CreateItemAsync(item);
            if (result != null)
                await _navigationService.NavigateBackAsync();
            else
            {
                ErrorMessage = "Failed to create item. Please try again.";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsNotLoading));
        }
    }
}