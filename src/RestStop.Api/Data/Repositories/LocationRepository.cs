namespace RestStop.Api.Data.Repositories;

/// <summary>
/// Concrete EF Core implementation of <see cref="ILocationRepository"/>.
/// Uses PostGIS spatial functions for radius-based queries.
/// </summary>
public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _db;

    /// <summary>Initialises a new instance of <see cref="LocationRepository"/>.</summary>
    /// <param name="db">The database context.</param>
    public LocationRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<List<LocationSummaryDto>> GetNearbyAsync(
        double lat, double lng, int radiusMetres)
    {
        var origin = new Point(lng, lat) { SRID = 4326 };

        return await _db.Locations
            .Where(l => l.IsActive && l.Coordinates.Distance(origin) <= radiusMetres)
            .OrderBy(l => l.Coordinates.Distance(origin))
            .Select(l => new LocationSummaryDto(
                l.Id,
                l.Name,
                l.Brand,
                l.Coordinates.Y,
                l.Coordinates.X,
                l.Coordinates.Distance(origin),
                l.Restrooms.Any()
                    ? l.Restrooms.Average(r => r.AvgOverall)
                    : 0,
                l.Restrooms.Sum(r => r.ReviewCount),
                l.Is24Hr))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<LocationDetailDto?> GetByIdAsync(Guid locationId)
    {
        var location = await _db.Locations
            .Include(l => l.Restrooms)
            .FirstOrDefaultAsync(l => l.Id == locationId && l.IsActive);

        if (location is null) return null;

        return new LocationDetailDto(
            location.Id,
            location.Name,
            location.Address,
            location.Brand,
            location.Coordinates.Y,
            location.Coordinates.X,
            location.Is24Hr,
            location.HoursOpen,
            location.ClaimedByOwner,
            location.Restrooms.Select(r => new RestroomSummaryDto(
                r.Id,
                r.Type.ToString(),
                r.AvgCleanliness,
                r.AvgSmell,
                r.AvgSupplies,
                r.AvgOverall,
                r.ReviewCount,
                r.HasBabyStation,
                r.HasShower,
                r.IsAccessible)).ToList());
    }
}
