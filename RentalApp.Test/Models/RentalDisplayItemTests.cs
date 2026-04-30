using RentalApp.Database.Models;

namespace RentalApp.Test.Models;

public class RentalDisplayItemTests
{
    private RentalDisplayItem CreateItem(string status, DateTime endDate, bool isIncoming = false)
    {
        var rental = new Rental { Status = status, EndDate = endDate };
        return new RentalDisplayItem(rental, isIncoming);
    }

    // -------------------------------------------------------------------------
    // IsOverdue
    // -------------------------------------------------------------------------

    [Fact]
    public void IsOverdue_WhenOutForRentAndPastEndDate_ReturnsTrue()
    {
        // Arrange
        var item = CreateItem("Out for Rent", DateTime.Today.AddDays(-1));

        // Assert
        Assert.True(item.IsOverdue);
    }

    [Fact]
    public void IsOverdue_WhenOutForRentAndEndDateToday_ReturnsFalse()
    {
        // Arrange
        var item = CreateItem("Out for Rent", DateTime.Today);

        // Assert
        Assert.False(item.IsOverdue);
    }

    [Fact]
    public void IsOverdue_WhenOutForRentAndFutureEndDate_ReturnsFalse()
    {
        // Arrange
        var item = CreateItem("Out for Rent", DateTime.Today.AddDays(1));

        // Assert
        Assert.False(item.IsOverdue);
    }

    [Theory]
    [InlineData("Returned")]
    [InlineData("Completed")]
    [InlineData("Approved")]
    [InlineData("Requested")]
    public void IsOverdue_WhenNotOutForRent_ReturnsFalse(string status)
    {
        // Arrange
        var item = CreateItem(status, DateTime.Today.AddDays(-1));

        // Assert
        Assert.False(item.IsOverdue);
    }

    // -------------------------------------------------------------------------
    // ShowApproveReject
    // -------------------------------------------------------------------------

    [Fact]
    public void ShowApproveReject_WhenRequestedAndIncoming_ReturnsTrue()
    {
        var item = CreateItem("Requested", DateTime.Today, isIncoming: true);
        Assert.True(item.ShowApproveReject);
    }

    [Fact]
    public void ShowApproveReject_WhenRequestedAndOutgoing_ReturnsFalse()
    {
        var item = CreateItem("Requested", DateTime.Today, isIncoming: false);
        Assert.False(item.ShowApproveReject);
    }

    // -------------------------------------------------------------------------
    // ShowMarkReturned
    // -------------------------------------------------------------------------

    [Fact]
    public void ShowMarkReturned_WhenOutForRentAndOutgoing_ReturnsTrue()
    {
        var item = CreateItem("Out for Rent", DateTime.Today, isIncoming: false);
        Assert.True(item.ShowMarkReturned);
    }

    [Fact]
    public void ShowMarkReturned_WhenOutForRentAndIncoming_ReturnsFalse()
    {
        var item = CreateItem("Out for Rent", DateTime.Today, isIncoming: true);
        Assert.False(item.ShowMarkReturned);
    }

    // -------------------------------------------------------------------------
    // ShowLeaveReview
    // -------------------------------------------------------------------------

    [Fact]
    public void ShowLeaveReview_WhenCompletedAndOutgoing_ReturnsTrue()
    {
        var item = CreateItem("Completed", DateTime.Today, isIncoming: false);
        Assert.True(item.ShowLeaveReview);
    }

    [Fact]
    public void ShowLeaveReview_WhenCompletedAndIncoming_ReturnsFalse()
    {
        var item = CreateItem("Completed", DateTime.Today, isIncoming: true);
        Assert.False(item.ShowLeaveReview);
    }
}
