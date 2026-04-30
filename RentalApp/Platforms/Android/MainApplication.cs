using Android.App;
using Android.Runtime;

namespace RentalApp;

/// <summary>
/// The Android Application class. Hooks into the global unhandled exception event
/// to log crashes to logcat before the process dies — useful during development
/// when ADB is attached and you need to see what crashed.
/// </summary>
[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        // Log any unhandled exception to logcat with the tag "CRASH" before marking
        // it handled so Android doesn't show the system crash dialog during demos.
        AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            Android.Util.Log.Error("CRASH", args.Exception.ToString());
            args.Handled = true;
        };
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
