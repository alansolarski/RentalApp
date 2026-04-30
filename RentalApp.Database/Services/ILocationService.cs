namespace RentalApp.Database.Services;

/// <summary>
/// Abstraction over device location. Lives here in RentalApp.Database so that
/// NearbyItemsViewModel (and tests) can depend on it without referencing MAUI APIs.
/// </summary>
/// <remarks>
/// The real implementation (LocationService in RentalApp/Services/) calls
/// Geolocation.GetLocationAsync, which requires a running MAUI runtime and actual
/// device hardware. It can't be tested in xUnit. LocationServiceTests mocks this
/// interface instead to verify the NearbyItems workflow without needing a device.
/// </remarks>
public interface ILocationService
{
    /// <summary>
    /// Returns the device's current GPS coordinates, or null if permission was denied
    /// or the location couldn't be determined.
    /// </summary>
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
}
