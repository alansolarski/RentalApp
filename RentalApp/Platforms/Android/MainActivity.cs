using Android.App;
using Android.Content.PM;
using Android.OS;

namespace RentalApp;

/// <summary>
/// The single Android Activity for this app. MAUI runs everything through one activity,
/// so there's nothing to add here beyond what the template provides.
/// ConfigurationChanges lists the events we handle ourselves so Android doesn't restart
/// the activity (and lose state) when the screen rotates or the system UI mode changes.
/// </summary>
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
