using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// Placeholder ViewModel for pages that haven't been built out yet.
/// Profile and Settings navigation from MainPage and AppShell currently land here.
/// </summary>
public class TempViewModel
{
    public string Title => AppInfo.Name;
    public string Version => AppInfo.VersionString;
    public string Message => "This is a placeholder page.";

    public TempViewModel()
    { }
}
