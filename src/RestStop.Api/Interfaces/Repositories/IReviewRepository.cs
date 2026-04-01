namespace RestStop.Api.Interfaces.Repositories;

/// <summary>
/// Defines data access operations for the Reviews table.
/// </summary>
public interface IReviewRepository
{
    /// <summary>
    /// Persists a new review to the database.
    /// </summary>
    /// <param name="review">The review entity to create.</param>
    /// <returns>The created <see cref="Review"/>.</returns>
    Task<Review> CreateAsync(Review review);

    /// <summary>
    /// Returns the number of reviews submitted by a user for a specific restroom
    /// within the last hour. Used for rate limiting.
    /// </summary>
    /// <param name="userId">The user's identifier.</param>
    /// <param name="restroomId">The restroom's identifier.</param>
    /// <returns>Count of reviews within the last hour.</returns>
    Task<int> GetRecentCountAsync(Guid userId, Guid restroomId);
}
