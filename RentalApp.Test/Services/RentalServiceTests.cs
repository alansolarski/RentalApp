using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.Test.Services;

public class RentalServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly RentalService _sut;

    public RentalServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _sut = new RentalService(_mockApiService.Object);
    }

    // -------------------------------------------------------------------------
    // RequestRentalAsync — date validation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RequestRentalAsync_StartDateInPast_ReturnsFalseWithError()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(2);

        // Act
        var (success, error) = await _sut.RequestRentalAsync(1, startDate, endDate);

        // Assert
        Assert.False(success);
        Assert.Equal("Start date cannot be in the past.", error);
    }

    [Fact]
    public async Task RequestRentalAsync_EndDateSameAsStartDate_ReturnsFalseWithError()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = startDate;

        // Act
        var (success, error) = await _sut.RequestRentalAsync(1, startDate, endDate);

        // Assert
        Assert.False(success);
        Assert.Equal("End date must be after start date.", error);
    }

    [Fact]
    public async Task RequestRentalAsync_EndDateBeforeStartDate_ReturnsFalseWithError()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(3);
        var endDate = DateTime.Today.AddDays(1);

        // Act
        var (success, error) = await _sut.RequestRentalAsync(1, startDate, endDate);

        // Assert
        Assert.False(success);
        Assert.Equal("End date must be after start date.", error);
    }

    [Fact]
    public async Task RequestRentalAsync_ValidDates_CallsApiServiceAndReturnsResult()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);
        _mockApiService
            .Setup(a => a.CreateRentalAsync(1, startDate, endDate))
            .ReturnsAsync((true, string.Empty));

        // Act
        var (success, error) = await _sut.RequestRentalAsync(1, startDate, endDate);

        // Assert
        Assert.True(success);
        _mockApiService.Verify(a => a.CreateRentalAsync(1, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task RequestRentalAsync_ValidDates_ApiFailure_ReturnsFalseWithError()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);
        _mockApiService
            .Setup(a => a.CreateRentalAsync(1, startDate, endDate))
            .ReturnsAsync((false, "Item is not available for the selected dates"));

        // Act
        var (success, error) = await _sut.RequestRentalAsync(1, startDate, endDate);

        // Assert
        Assert.False(success);
        Assert.Equal("Item is not available for the selected dates", error);
    }

    // -------------------------------------------------------------------------
    // UpdateRentalStatusAsync — status validation
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    [InlineData("Returned")]
    [InlineData("Completed")]
    public async Task UpdateRentalStatusAsync_ValidStatus_CallsApiService(string status)
    {
        // Arrange
        _mockApiService
            .Setup(a => a.UpdateRentalStatusAsync(1, status))
            .ReturnsAsync((true, string.Empty));

        // Act
        var (success, error) = await _sut.UpdateRentalStatusAsync(1, status);

        // Assert
        Assert.True(success);
        _mockApiService.Verify(a => a.UpdateRentalStatusAsync(1, status), Times.Once);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Cancelled")]
    [InlineData("")]
    [InlineData("random")]
    public async Task UpdateRentalStatusAsync_InvalidStatus_ReturnsFalseWithoutCallingApi(string status)
    {
        // Arrange & Act
        var (success, error) = await _sut.UpdateRentalStatusAsync(1, status);

        // Assert
        Assert.False(success);
        Assert.Equal("Invalid status.", error);
        _mockApiService.Verify(a => a.UpdateRentalStatusAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    // -------------------------------------------------------------------------
    // GetIncomingRentalsAsync / GetOutgoingRentalsAsync — delegation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetIncomingRentalsAsync_ReturnsDelegatedApiResult()
    {
        // Arrange
        var rentals = new List<Rental> { new Rental { Id = 1, ItemTitle = "Drill" } };
        _mockApiService.Setup(a => a.GetIncomingRentalsAsync()).ReturnsAsync(rentals);

        // Act
        var result = await _sut.GetIncomingRentalsAsync();

        // Assert
        Assert.Equal(rentals, result);
        _mockApiService.Verify(a => a.GetIncomingRentalsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetOutgoingRentalsAsync_ReturnsDelegatedApiResult()
    {
        // Arrange
        var rentals = new List<Rental> { new Rental { Id = 2, ItemTitle = "Tent" } };
        _mockApiService.Setup(a => a.GetOutgoingRentalsAsync()).ReturnsAsync(rentals);

        // Act
        var result = await _sut.GetOutgoingRentalsAsync();

        // Assert
        Assert.Equal(rentals, result);
        _mockApiService.Verify(a => a.GetOutgoingRentalsAsync(), Times.Once);
    }
}