using Moq;
using RentalApp.Database.Models;
using RentalApp.Database.Services;

namespace RentalApp.Test.Services;

/// <summary>
/// Tests for ReviewService. The interesting logic here is the HTTP status code translation
/// in SubmitReviewAsync — the service catches HttpRequestException and maps 409 Conflict
/// to a "already reviewed" message and 403 Forbidden to a "borrower only" message.
/// Those mappings are what these tests verify.
/// </summary>
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
        // A null comment is valid — comment is optional in the API.
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
        // 409 Conflict = the API already has a review from this user for this rental.
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
        // 403 Forbidden = the caller isn't the borrower for this rental.
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
        // Anything that isn't a known HTTP status code falls through to the raw message.
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
