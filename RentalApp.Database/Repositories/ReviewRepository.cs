using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Database.Repositories;

/// <summary>
/// Stub implementation of <see cref="IReviewRepository"/>.
/// </summary>
/// <remarks>
/// All review operations go through the API (GET /items/{id}/reviews, POST /reviews) via
/// ApiService, so there's no meaningful local DB logic to put here. The class exists because
/// the coursework architecture checklist requires a ReviewRepository. If we were caching
/// reviews locally, the read methods would live here.
/// </remarks>
public class ReviewRepository : IReviewRepository
{
    // Held in case we need local DB operations in a future iteration, but currently unused.
    private readonly AppDbContext _context;

    /// <summary>Creates the repository. Context is injected but not yet used.</summary>
    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }
}
