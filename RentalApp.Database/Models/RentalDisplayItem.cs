namespace RentalApp.Database.Models;

public class RentalDisplayItem
{
    public Rental Rental { get; }
    public bool IsIncoming { get; }
    public bool IsOutgoing => !IsIncoming;
    public bool ShowApproveReject => Rental.Status == "Requested" && IsIncoming;
    public bool ShowMarkReturned => Rental.Status == "Out for Rent" && IsOutgoing;
    public bool ShowLeaveReview => IsOutgoing && Rental.Status == "Completed";
    public bool IsOverdue => Rental.Status == "Out for Rent" && Rental.EndDate < DateTime.Today;

    public RentalDisplayItem(Rental rental, bool isIncoming)
    {
        Rental = rental;
        IsIncoming = isIncoming;
    }
}
