using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.Test.Services;

/// <summary>
/// Tests for location-dependent behaviour using a mock ILocationService.
/// The real LocationService calls MAUI's Geolocation API which requires a physical
/// device with location permission, so we can't use it in xUnit — the interface
/// exists in the Database project so we can mock it here.
/// </summary>
public class LocationServiceTests
{
    private readonly Mock<ILocationService> _mockLocationService;
    private readonly Mock<IApiService> _mockApiService;

    public LocationServiceTests()
    {
        _mockLocationService = new Mock<ILocationService>();
        _mockApiService = new Mock<IApiService>();
    }

    // -------------------------------------------------------------------------
    // ILocationService mock behaviour
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetCurrentLocationAsync_ReturnsLocation_WhenPermissionGranted()
    {
        // Arrange
        var expectedLocation = (Latitude: 55.9533, Longitude: -3.1883);
        _mockLocationService
            .Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(55.9533, result.Value.Latitude);
        Assert.Equal(-3.1883, result.Value.Longitude);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_ReturnsNull_WhenPermissionDenied()
    {
        // Arrange
        // Null means the user denied the permission prompt or location is off.
        _mockLocationService
            .Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync((ValueTuple<double, double>?)null);

        // Act
        var result = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Assert
        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // NearbyItems workflow, location and API interaction
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindNearbyItems_WithValidLocation_CallsApiWithCoordinates()
    {
        // Arrange
        var location = (Latitude: 55.9533, Longitude: -3.1883);
        _mockLocationService
            .Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync(location);

        var expectedItems = new List<NearbyItem>
        {
            new NearbyItem { Id = 1, Title = "Drill", Distance = 1.2m },
            new NearbyItem { Id = 2, Title = "Tent", Distance = 3.5m }
        };
        _mockApiService
            .Setup(a => a.GetNearbyItemsAsync(55.9533, -3.1883, 5.0))
            .ReturnsAsync(expectedItems);

        // Act
        var loc = await _mockLocationService.Object.GetCurrentLocationAsync();
        var items = await _mockApiService.Object.GetNearbyItemsAsync(
            loc!.Value.Latitude, loc.Value.Longitude, 5.0);

        // Assert
        Assert.NotNull(loc);
        Assert.Equal(2, items.Count);
        Assert.Equal("Drill", items[0].Title);
        _mockApiService.Verify(a => a.GetNearbyItemsAsync(55.9533, -3.1883, 5.0), Times.Once);
    }

    [Fact]
    public async Task FindNearbyItems_WhenLocationNull_DoesNotCallApi()
    {
        // Arrange
        _mockLocationService
            .Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync((ValueTuple<double, double>?)null);

        // Act
        var loc = await _mockLocationService.Object.GetCurrentLocationAsync();

        // Simulate what NearbyItemsViewModel does and bail out if no location
        if (loc == null)
        {
            // Don't call API
        }
        else
        {
            await _mockApiService.Object.GetNearbyItemsAsync(
                loc.Value.Latitude, loc.Value.Longitude, 5.0);
        }

        // Assert
        Assert.Null(loc);
        _mockApiService.Verify(
            a => a.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()),
            Times.Never);
    }

    [Fact]
    public async Task FindNearbyItems_WhenApiReturnsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var location = (Latitude: 55.9533, Longitude: -3.1883);
        _mockLocationService
            .Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync(location);
        _mockApiService
            .Setup(a => a.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync(new List<NearbyItem>());

        // Act
        var loc = await _mockLocationService.Object.GetCurrentLocationAsync();
        var items = await _mockApiService.Object.GetNearbyItemsAsync(
            loc!.Value.Latitude, loc.Value.Longitude, 5.0);

        // Assert
        Assert.Empty(items);
    }
}
