using RentalApp.Database.Models;

namespace RentalApp.Database.Services;

/// <summary>
/// Contract for all HTTP calls to the rental API (https://set09102-api.b-davison.workers.dev/).
/// Lives here in RentalApp.Database so that ViewModels in this project (and tests) can
/// depend on it without referencing the MAUI project.
/// </summary>
/// <remarks>
/// ApiService is excluded from code coverage in coverlet.runsettings because it makes live
/// HTTP calls. Tests mock this interface instead.
/// </remarks>
public interface IApiService
{
    // ----- Items -----

    /// <summary>Returns all available items from GET /items.</summary>
    Task<IEnumerable<Item>> GetItemsAsync();

    /// <summary>Returns a single item from GET /items/{id}, or null if not found.</summary>
    Task<Item?> GetItemByIdAsync(int id);

    /// <summary>Creates a new item via POST /items. Returns the created item or null on failure.</summary>
    Task<Item?> CreateItemAsync(Item item);

    /// <summary>Returns all categories from GET /categories.</summary>
    Task<IEnumerable<Category>> GetCategoriesAsync();

    /// <summary>Updates an item via PUT /items/{id}. Returns true on success.</summary>
    Task<bool> UpdateItemAsync(int id, UpdateItemRequest request);

    /// <summary>Returns items near a location from GET /items/nearby.</summary>
    /// <param name="lat">Latitude of the search centre.</param>
    /// <param name="lon">Longitude of the search centre.</param>
    /// <param name="radiusKm">Search radius in kilometres. Defaults to 5.</param>
    Task<List<NearbyItem>> GetNearbyItemsAsync(double lat, double lon, double radiusKm = 5);

    // ----- Rentals -----

    /// <summary>Creates a rental request via POST /rentals.</summary>
    /// <returns>Success flag and an error message if it failed.</returns>
    Task<(bool Success, string Error)> CreateRentalAsync(int itemId, DateTime startDate, DateTime endDate);

    /// <summary>Returns rentals where the current user is the owner (GET /rentals/incoming).</summary>
    Task<List<Rental>> GetIncomingRentalsAsync();

    /// <summary>Returns rentals where the current user is the borrower (GET /rentals/outgoing).</summary>
    Task<List<Rental>> GetOutgoingRentalsAsync();

    /// <summary>Updates a rental's status via PATCH /rentals/{id}/status.</summary>
    /// <returns>Success flag and an error message if it failed.</returns>
    Task<(bool Success, string Error)> UpdateRentalStatusAsync(int rentalId, string status);

    // ----- Reviews -----

    /// <summary>Returns all reviews for an item from GET /items/{id}/reviews.</summary>
    Task<IEnumerable<Review>> GetItemReviewsAsync(int itemId);

    /// <summary>Submits a review via POST /reviews. Throws on HTTP errors (caller handles them).</summary>
    /// <param name="rentalId">The rental being reviewed.</param>
    /// <param name="rating">Star rating 1–5.</param>
    /// <param name="comment">Optional free-text comment.</param>
    Task<Review> SubmitReviewAsync(int rentalId, int rating, string? comment);

    // ----- Profile -----

    /// <summary>Returns the current user's profile from GET /users/me. Returns null if the request fails.</summary>
    Task<UserProfile?> GetCurrentUserProfileAsync();
}
