namespace RentalApp.Database.Data.Repositories;

/// <summary>
/// Marker interface for the review repository. Currently empty because all review
/// logic goes through the API (POST /reviews, GET /items/{id}/reviews) rather than
/// the local database.
/// </summary>
/// <remarks>
/// This interface exists to satisfy the coursework architecture checklist requirement
/// for a ReviewRepository. There's no meaningful local DB operation for reviews right now.
/// If we added offline caching later, the read methods would go here.
/// </remarks>
public interface IReviewRepository
{
    // no local DB operations needed — all review reads/writes go through ApiService
}
