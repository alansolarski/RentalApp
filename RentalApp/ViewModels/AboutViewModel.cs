using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// ViewModel for the About page. Exposes app name, version, and a command to open
/// the MAUI documentation URL in the device browser.
/// </summary>
public class AboutViewModel
{
    /// <summary>App name from AppInfo — automatically populated from the manifest.</summary>
    public string Title => AppInfo.Name;

    /// <summary>Version string from AppInfo, e.g. "1.0.0".</summary>
    public string Version => AppInfo.VersionString;

    public string MoreInfoUrl => "https://aka.ms/maui";
    public string Message => "This app is written in XAML and C# with .NET MAUI.";

    /// <summary>Opens MoreInfoUrl in the default browser.</summary>
    public ICommand ShowMoreInfoCommand { get; }

    public AboutViewModel()
    {
        ShowMoreInfoCommand = new AsyncRelayCommand(ShowMoreInfo);
    }

    private async Task ShowMoreInfo() =>
        await Launcher.Default.OpenAsync(MoreInfoUrl);
}
