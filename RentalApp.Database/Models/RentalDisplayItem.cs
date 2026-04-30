namespace RentalApp.Database.Models;

/// <summary>
/// Wrapper around a <see cref="Rental"/> that adds UI-driven computed properties like
/// which action buttons to show and whether the rental is overdue.
/// </summary>
/// <remarks>
/// This lives in RentalApp.Database (rather than inline in RentalsViewModel) because
/// the computed properties — especially IsOverdue — needed unit tests. The test project
/// can't reference the MAUI project (it'd pull in Android targets), so anything that
/// needs testing has to live in the shared Database project.
/// </remarks>
public class RentalDisplayItem
{
    /// <summary>The underlying rental data from the API.</summary>
    public Rental Rental { get; }

    /// <summary>True if this rental is coming in to the current user as owner.</summary>
    public bool IsIncoming { get; }

    /// <summary>Convenience inverse of IsIncoming — rental is going out from the current user as borrower.</summary>
    public bool IsOutgoing => !IsIncoming;

    /// <summary>
    /// True when the owner needs to approve or reject this rental.
    /// Only relevant for incoming (owner-side) rentals in "Requested" status.
    /// </summary>
    public bool ShowApproveReject => Rental.Status == "Requested" && IsIncoming;

    /// <summary>
    /// True when the borrower can mark this as returned.
    /// Only the borrower (outgoing side) can trigger "Returned" status.
    /// </summary>
    public bool ShowMarkReturned => Rental.Status == "Out for Rent" && IsOutgoing;

    /// <summary>True when the borrower can leave a review — only after the rental is completed.</summary>
    public bool ShowLeaveReview => IsOutgoing && Rental.Status == "Completed";

    /// <summary>
    /// True when an active rental has passed its end date without being returned.
    /// Uses DateTime.Today (date only) to avoid time-of-day false positives.
    /// </summary>
    public bool IsOverdue => Rental.Status == "Out for Rent" && Rental.EndDate < DateTime.Today;

    /// <summary>
    /// Creates a display wrapper for the given rental.
    /// </summary>
    /// <param name="rental">The rental data from the API.</param>
    /// <param name="isIncoming">True if this rental is incoming (current user is the owner).</param>
    public RentalDisplayItem(Rental rental, bool isIncoming)
    {
        Rental = rental;
        IsIncoming = isIncoming;
    }
}
