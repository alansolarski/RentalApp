using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Services;
using RentalApp.Views;
using System.Collections.ObjectModel;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the Rentals page. Manages incoming (owner-side) and outgoing (borrower-side)
/// rental lists, the tab toggle between them, and action commands (approve, reject, return, review).
/// </summary>
public partial class RentalsViewModel : ObservableObject
{
    private readonly IRentalService _rentalService;
    private readonly TokenStore _tokenStore;

    // Both lists are fetched in a single LoadRentals call and cached here.
    private List<Rental> _incomingRentals = new();
    private List<Rental> _outgoingRentals = new();
    private bool _showingIncoming = true;

    [ObservableProperty]
    private ObservableCollection<RentalDisplayItem> _activeRentals = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Button colours toggle to give a visual tab-indicator effect.
    [ObservableProperty]
    private Color _incomingButtonColor = Colors.Purple;

    [ObservableProperty]
    private Color _outgoingButtonColor = Colors.Gray;

    public bool ShowingIncoming => _showingIncoming;
    public bool ShowingOutgoing => !_showingIncoming;

    /// <summary>The logged-in user's numeric ID — used in the XAML to colour-code rental rows.</summary>
    public int CurrentUserId => _tokenStore.UserId;

    public RentalsViewModel(IRentalService rentalService, TokenStore tokenStore)
    {
        _rentalService = rentalService;
        _tokenStore = tokenStore;
    }

    /// <summary>
    /// Fetches both incoming and outgoing rentals in parallel and refreshes the displayed list.
    /// Called from RentalsPage.OnAppearing so the list is always fresh.
    /// </summary>
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

    /// <summary>Switches the view to the incoming (owner) tab.</summary>
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

    /// <summary>Switches the view to the outgoing (borrower) tab.</summary>
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

    /// <summary>
    /// Rebuilds ActiveRentals from whichever list is currently shown.
    /// Wraps each Rental in a RentalDisplayItem so the XAML can bind to
    /// computed properties like ShowApproveReject and IsOverdue.
    /// </summary>
    private void RefreshActiveList()
    {
        ActiveRentals.Clear();
        var source = _showingIncoming ? _incomingRentals : _outgoingRentals;
        foreach (var r in source)
            ActiveRentals.Add(new RentalDisplayItem(r, _showingIncoming));
    }

    /// <summary>Approves a rental request and reloads the list.</summary>
    [RelayCommand]
    private async Task ApproveRentalAsync(RentalDisplayItem item)
    {
        var (success, error) = await _rentalService.UpdateRentalStatusAsync(item.Rental.Id, "Approved");
        if (success)
            await LoadRentalsAsync();
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }

    /// <summary>Rejects a rental request and reloads the list.</summary>
    [RelayCommand]
    private async Task RejectRentalAsync(RentalDisplayItem item)
    {
        var (success, error) = await _rentalService.UpdateRentalStatusAsync(item.Rental.Id, "Rejected");
        if (success)
            await LoadRentalsAsync();
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }

    /// <summary>Marks a rental as returned (borrower action) and reloads the list.</summary>
    [RelayCommand]
    private async Task MarkReturnedAsync(RentalDisplayItem item)
    {
        var (success, error) = await _rentalService.UpdateRentalStatusAsync(item.Rental.Id, "Returned");
        if (success)
            await LoadRentalsAsync();
        else
            await Shell.Current.DisplayAlert("Error", error, "OK");
    }

    /// <summary>Navigates to the Reviews page with canReview=true so the review form is shown.</summary>
    [RelayCommand]
    private async Task LeaveReviewAsync(RentalDisplayItem item)
    {
        await Shell.Current.GoToAsync(
            $"{nameof(ReviewsPage)}?itemId={item.Rental.ItemId}&rentalId={item.Rental.Id}&canReview=true");
    }
}
