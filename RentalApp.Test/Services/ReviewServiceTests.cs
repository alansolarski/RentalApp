using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.Test.Services;

public class ReviewServiceTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        _mockApiService = new Mock<IApiService>();
        _sut = new ReviewService(_mockApiService.Object);
    }

    // -------------------------------------------------------------------------
    // SubmitReviewAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SubmitReviewAsync_Success_ReturnsTrueWithEmptyError()
    {
        // Arrange
        _mockApiService
            .Setup(a => a.SubmitReviewAsync(1, 5, "Great item!"))
            .ReturnsAsync(new Review());

        // Act
        var (success, error) = await _sut.SubmitReviewAsync(1, 5, "Great item!");

        // Assert
        Assert.True(success);
        Assert.Equal(string.Empty, error);
        _mockApiService.Verify(a => a.SubmitReviewAsync(1, 5, "Great item!"), Times.Once);
    }

    [Fact]
    public async Task SubmitReviewAsync_NullComment_CallsApiSuccessfully()
    {
        // Arrange
        _mockApiService
            .Setup(a => a.SubmitReviewAsync(1, 3, null))
            .ReturnsAsync(new Review());

        // Act
        var (success, error) = await _sut.SubmitReviewAsync(1, 3, null);

        // Assert
        Assert.True(success);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public async Task SubmitReviewAsync_ConflictException_ReturnsFalseWithAlreadyReviewedError()
    {
        // Arrange
        _mockApiService
            .Setup(a => a.SubmitReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
            .ThrowsAsync(new HttpRequestException("Conflict", null, System.Net.HttpStatusCode.Conflict));

        // Act
        var (success, error) = await _sut.SubmitReviewAsync(1, 5, "Great!");

        // Assert
        Assert.False(success);
        Assert.Equal("You have already reviewed this rental.", error);
    }

    [Fact]
    public async Task SubmitReviewAsync_ForbiddenException_ReturnsFalseWithForbiddenError()
    {
        // Arrange
        _mockApiService
            .Setup(a => a.SubmitReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
            .ThrowsAsync(new HttpRequestException("Forbidden", null, System.Net.HttpStatusCode.Forbidden));

        // Act
        var (success, error) = await _sut.SubmitReviewAsync(1, 5, "Great!");

        // Assert
        Assert.False(success);
        Assert.Equal("Only the borrower can review a completed rental.", error);
    }

    [Fact]
    public async Task SubmitReviewAsync_UnexpectedException_ReturnsFalseWithExceptionMessage()
    {
        // Arrange
        _mockApiService
            .Setup(a => a.SubmitReviewAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("Network timeout"));

        // Act
        var (success, error) = await _sut.SubmitReviewAsync(1, 5, "Great!");

        // Assert
        Assert.False(success);
        Assert.Equal("Network timeout", error);
    }

    // -------------------------------------------------------------------------
    // GetItemReviewsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetItemReviewsAsync_ReturnsDelegatedApiResult()
    {
        // Arrange
        var reviews = new List<Review>
        {
            new Review { Id = 1, Rating = 5, Comment = "Excellent!" },
            new Review { Id = 2, Rating = 3, Comment = "Good enough." }
        };
        _mockApiService
            .Setup(a => a.GetItemReviewsAsync(42))
            .ReturnsAsync(reviews.AsEnumerable());

        // Act
        var result = await _sut.GetItemReviewsAsync(42);

        // Assert
        Assert.Equal(2, result.Count());
        _mockApiService.Verify(a => a.GetItemReviewsAsync(42), Times.Once);
    }

    [Fact]
    public async Task GetItemReviewsAsync_EmptyList_ReturnsEmpty()
    {
        // Arrange
        _mockApiService
            .Setup(a => a.GetItemReviewsAsync(It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<Review>());

        // Act
        var result = await _sut.GetItemReviewsAsync(1);

        // Assert
        Assert.Empty(result);
    }
}