using RentalApp.Database.Models;

namespace RentalApp.Test.Models;

/// <summary>
/// Tests for the computed display properties on RentalDisplayItem.
/// RentalDisplayItem lives in the Database project specifically so these tests can reach it
/// without taking a dependency on the MAUI project (which can't run in xUnit).
/// </summary>
public class RentalDisplayItemTests
{
    /// <summary>Helper to build a RentalDisplayItem from a status string and end date.</summary>
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
        // EndDate == Today means the return is due today — not yet overdue.
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
        // Past end date but status isn't "Out for Rent" — can't be overdue.
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
        // Only the owner (incoming side) can approve or reject — borrowers can't.
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
        // The owner doesn't mark the item returned — that's the borrower's action.
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
        // Only the borrower (outgoing side) can leave a review — owners don't.
        var item = CreateItem("Completed", DateTime.Today, isIncoming: true);
        Assert.False(item.ShowLeaveReview);
    }
}
