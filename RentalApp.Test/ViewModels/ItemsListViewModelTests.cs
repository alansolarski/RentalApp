using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Services;
using RentalApp.Database.ViewModels;

namespace RentalApp.Test.ViewModels;

/// <summary>
/// Tests for ItemsListViewModel (the Database project copy).
/// The ViewModel is duplicated in both the Database project and the MAUI project.
/// Tests reference the Database project copy because the MAUI project can't run in xUnit
/// (it pulls in Android targets). Both copies must be kept manually in sync.
/// </summary>
public class ItemsListViewModelTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly ItemsListViewModel _sut;

    public ItemsListViewModelTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockNavigationService = new Mock<INavigationService>();
        _sut = new ItemsListViewModel(_mockApiService.Object, _mockNavigationService.Object);
    }

    // -------------------------------------------------------------------------
    // LoadItemsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task LoadItemsAsync_Success_PopulatesItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = 1, Title = "Drill" },
            new Item { Id = 2, Title = "Tent" }
        };
        _mockApiService.Setup(a => a.GetItemsAsync()).ReturnsAsync(items);

        // Act
        await _sut.LoadItemsAsync();

        // Assert
        Assert.Equal(2, _sut.Items.Count);
        Assert.Equal("Drill", _sut.Items[0].Title);
        Assert.Equal("Tent", _sut.Items[1].Title);
    }

    [Fact]
    public async Task LoadItemsAsync_Success_ClearsErrorMessage()
    {
        // Arrange
        _mockApiService.Setup(a => a.GetItemsAsync()).ReturnsAsync(new List<Item>());

        // Act
        await _sut.LoadItemsAsync();

        // Assert
        Assert.Equal(string.Empty, _sut.ErrorMessage);
    }

    [Fact]
    public async Task LoadItemsAsync_Success_SetsIsLoadingFalseWhenDone()
    {
        // Arrange
        _mockApiService.Setup(a => a.GetItemsAsync()).ReturnsAsync(new List<Item>());

        // Act
        await _sut.LoadItemsAsync();

        // Assert
        Assert.False(_sut.IsLoading);
    }

    [Fact]
    public async Task LoadItemsAsync_ApiThrows_SetsErrorMessage()
    {
        // Arrange
        _mockApiService
            .Setup(a => a.GetItemsAsync())
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await _sut.LoadItemsAsync();

        // Assert
        Assert.Equal("Failed to load items: Network error", _sut.ErrorMessage);
    }

    [Fact]
    public async Task LoadItemsAsync_ApiThrows_SetsIsLoadingFalse()
    {
        // Arrange
        // IsLoading must return to false even when an exception is thrown,
        // otherwise the spinner stays visible and the UI looks broken.
        _mockApiService
            .Setup(a => a.GetItemsAsync())
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await _sut.LoadItemsAsync();

        // Assert
        Assert.False(_sut.IsLoading);
    }

    [Fact]
    public async Task LoadItemsAsync_EmptyList_ItemsCollectionIsEmpty()
    {
        // Arrange
        _mockApiService.Setup(a => a.GetItemsAsync()).ReturnsAsync(new List<Item>());

        // Act
        await _sut.LoadItemsAsync();

        // Assert
        Assert.Empty(_sut.Items);
    }

    // -------------------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NavigateToCreateAsync_CallsNavigationService()
    {
        // Arrange
        _mockNavigationService
            .Setup(n => n.NavigateToAsync("CreateItemPage"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.NavigateToCreateAsync();

        // Assert
        _mockNavigationService.Verify(n => n.NavigateToAsync("CreateItemPage"), Times.Once);
    }

    [Fact]
    public async Task NavigateToDetailAsync_CallsNavigationServiceWithId()
    {
        // Arrange
        _mockNavigationService
            .Setup(n => n.NavigateToAsync("ItemDetailPage?id=42"))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.NavigateToDetailAsync(42);

        // Assert
        _mockNavigationService.Verify(n => n.NavigateToAsync("ItemDetailPage?id=42"), Times.Once);
    }
}
