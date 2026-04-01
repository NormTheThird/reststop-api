namespace RestStop.Api.Services;

/// <summary>
/// Handles restroom review submission including GPS gating and weighted average updates.
/// </summary>
public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviews;
    private readonly AppDbContext _db;

    /// <summary>Initialises a new instance of <see cref="ReviewService"/>.</summary>
    /// <param name="reviews">Repository for review data access.</param>
    /// <param name="db">The database context used to fetch restroom and user data.</param>
    public ReviewService(IReviewRepository reviews, AppDbContext db)
    {
        _reviews = reviews;
        _db = db;
    }

    /// <inheritdoc />
    public async Task<ReviewDto> SubmitAsync(Guid userId, SubmitReviewRequest request)
    {
        var restroom = await _db.Restrooms
            .Include(r => r.Location)
            .FirstOrDefaultAsync(r => r.Id == request.RestroomId)
            ?? throw new KeyNotFoundException("Restroom not found.");

        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var locationLat = restroom.Location.Coordinates.Y;
        var locationLng = restroom.Location.Coordinates.X;

        if (!GeoHelper.IsWithinReviewRange(request.Lat, request.Lng, locationLat, locationLng))
            throw new InvalidOperationException("You must be at the location to submit a review.");

        var distance = GeoHelper.DistanceMetres(request.Lat, request.Lng, locationLat, locationLng);

        var review = new Review
        {
            UserId = userId,
            RestroomId = request.RestroomId,
            Cleanliness = request.Cleanliness,
            Smell = request.Smell,
            Supplies = request.Supplies,
            Overall = request.Overall,
            LatAtSubmit = request.Lat,
            LngAtSubmit = request.Lng,
            DistanceFromLocation = distance,
            WeightApplied = user.TrustWeight
        };

        var created = await _reviews.CreateAsync(review);

        var weightedCount = restroom.ReviewCount * 1.0;
        restroom.AvgCleanliness = TrustWeightHelper.UpdateWeightedAverage(restroom.AvgCleanliness, weightedCount, request.Cleanliness, user.TrustWeight);
        restroom.AvgSmell = TrustWeightHelper.UpdateWeightedAverage(restroom.AvgSmell, weightedCount, request.Smell, user.TrustWeight);
        restroom.AvgSupplies = TrustWeightHelper.UpdateWeightedAverage(restroom.AvgSupplies, weightedCount, request.Supplies, user.TrustWeight);
        restroom.AvgOverall = TrustWeightHelper.UpdateWeightedAverage(restroom.AvgOverall, weightedCount, request.Overall, user.TrustWeight);
        restroom.ReviewCount++;

        user.ReviewCount++;
        user.TrustWeight = TrustWeightHelper.Calculate(user.AccountAgeDays, user.ReviewCount, 0, 0);

        await _db.SaveChangesAsync();

        return new ReviewDto(
            created.Id,
            created.RestroomId,
            user.Username ?? "Anonymous",
            created.Cleanliness,
            created.Smell,
            created.Supplies,
            created.Overall,
            created.PhotoAttached,
            created.HelpfulVotes,
            created.CreatedAt);
    }
}
