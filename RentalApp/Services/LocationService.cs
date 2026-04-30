namespace RentalApp.Services;
using RentalApp.Database.Services;

/// <summary>
/// MAUI implementation of <see cref="ILocationService"/>. Uses the MAUI Geolocation API
/// to get the device's current GPS coordinates.
/// </summary>
/// <remarks>
/// This can't be unit tested because it calls MAUI APIs (Permissions.RequestAsync,
/// Geolocation.GetLocationAsync) that only work inside a running MAUI runtime on
/// an actual device or emulator. LocationServiceTests mocks ILocationService instead.
/// Tested manually on the Android emulator with a spoofed location.
///
/// Registered as a singleton in MauiProgram so there's only one location request
/// in flight at a time.
/// </remarks>
public class LocationService : ILocationService
{
    /// <summary>
    /// Requests location permission and returns the current coordinates, or null if
    /// permission was denied or the location couldn't be determined within 10 seconds.
    /// </summary>
    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        try
        {
            // Request permission first — if the user denies it, bail out immediately.
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location == null)
                return null;

            return (location.Latitude, location.Longitude);
        }
        catch (Exception)
        {
            // Swallow exceptions from the platform API (e.g. location services disabled)
            // and return null so the caller can show a friendly message.
            return null;
        }
    }
}
