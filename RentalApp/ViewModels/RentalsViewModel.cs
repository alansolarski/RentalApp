using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;
using RentalApp.Views;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

public partial class RentalsViewModel : ObservableObject
{
    private readonly IRentalService _rentalService;
    private readonly TokenStore _tokenStore;
    private List<Rental> _incomingRentals = new();
    private List<Rental> _outgoingRentals = new();
    private bool _showingIncoming = true;

    [ObservableProperty]
    private ObservableCollection<RentalDisplayItem> _activeRentals = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private Color _incomingButtonColor = Colors.Purple;

    [ObservableProperty]
    private Color _outgoingButtonColor = Colors.Gray;

    public bool ShowingIncoming => _showingIncoming;
    public bool ShowingOutgoing => !_showingIncoming;
    public int CurrentUserId => _tokenStore.UserId;

    public RentalsViewModel(IRentalService rentalService, TokenStore tokenStore)
    {
        _rentalService = rentalService;
        _tokenStore = tokenStore;
    }

    [RelayCommand]
    private async Task LoadRentalsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            _incomingRentals = await _rentalService.GetIncomingRentalsAsync();
            _outgoingRentals = await _rentalService.GetOutgoingRentalsAsync();
            RefreshActiveList();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowIncoming()
    {
        _showingIncoming = true;
        IncomingButtonColor = Colors.Purple;
        OutgoingButtonColor = Colors.Gray;
        OnPropertyChanged(nameof(ShowingIncoming));
        OnPropertyChanged(nameof(ShowingOutgoing));
        RefreshActiveList();
    }

    [RelayCommand]
    private void ShowOutgoing()
    {
        _showingIncoming = false;
        IncomingButtonColor = Colors.Gray;
        OutgoingButtonColor = Colors.Purple;
        OnPropertyChanged(nameof(ShowingIncoming));
        OnPropertyChanged(nameof(ShowingOutgoing));
        RefreshActiveList();
    }

    private void RefreshActiveList()
    {
        ActiveRentals.Clear();
        var source = _showingIncoming ? _incomingRentals : _outgoingRentals;
        foreach (var r in source)
            ActiveRentals.Add(new RentalDisplayItem(r, _showingIncoming));
    }

    [RelayCommand]
    private async Task ApproveRentalAsync(RentalDisplayItem item)
    {
        var (success, error) = await _rentalService.UpdateRentalStatusAsync(item.Rental.Id, "Approved");
        if (success)
            await LoadRentalsAsync();
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }

    [RelayCommand]
    private async Task RejectRentalAsync(RentalDisplayItem item)
    {
        var (success, error) = await _rentalService.UpdateRentalStatusAsync(item.Rental.Id, "Rejected");
        if (success)
            await LoadRentalsAsync();
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }

    [RelayCommand]
    private async Task MarkReturnedAsync(RentalDisplayItem item)
    {
        var (success, error) = await _rentalService.UpdateRentalStatusAsync(item.Rental.Id, "Returned");
        if (success)
            await LoadRentalsAsync();
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }

    [RelayCommand]
    private async Task LeaveReviewAsync(RentalDisplayItem item)
    {
        await Shell.Current.GoToAsync(
            $"{nameof(ReviewsPage)}?itemId={item.Rental.ItemId}&rentalId={item.Rental.Id}&canReview=true");
    }
}