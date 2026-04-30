using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// Base ViewModel that provides shared observable properties used across most pages:
/// IsBusy, Title, ErrorMessage, and HasError.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    /// <summary>True while an async operation is running. Bound to activity indicators in XAML.</summary>
    [ObservableProperty]
    private bool isBusy;

    /// <summary>Page title shown in the Shell navigation bar.</summary>
    [ObservableProperty]
    private string title = string.Empty;

    /// <summary>Current error message to show the user. Empty when there's no error.</summary>
    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>True when ErrorMessage is non-empty — used to toggle error UI visibility.</summary>
    [ObservableProperty]
    private bool hasError;

    /// <summary>Sets the error message and flips HasError to true.</summary>
    /// <param name="message">The error text to display.</param>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = !string.IsNullOrEmpty(message);
    }

    /// <summary>Clears any current error state.</summary>
    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    /// <summary>Relay command that clears the error state. Can be bound to a dismiss button.</summary>
    [RelayCommand]
    private void ClearErrorCommand()
    {
        ClearError();
    }
}
