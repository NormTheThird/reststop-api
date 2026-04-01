namespace RestStop.Api.Data.Repositories;

/// <summary>
/// Concrete EF Core implementation of <see cref="IReviewRepository"/>.
/// </summary>
public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _db;

    /// <summary>Initialises a new instance of <see cref="ReviewRepository"/>.</summary>
    /// <param name="db">The database context.</param>
    public ReviewRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<Review> CreateAsync(Review review)
    {
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return review;
    }

    /// <inheritdoc />
    public Task<int> GetRecentCountAsync(Guid userId, Guid restroomId)
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        return _db.Reviews.CountAsync(r =>
            r.UserId == userId &&
            r.RestroomId == restroomId &&
            r.CreatedAt >= oneHourAgo);
    }
}
