namespace RestStop.Api.Services;

/// <summary>
/// Handles all business logic for gas station location discovery.
/// </summary>
public class LocationService : ILocationService
{
    private readonly ILocationRepository _locations;

    /// <summary>Initialises a new instance of <see cref="LocationService"/>.</summary>
    /// <param name="locations">Repository for location data access.</param>
    public LocationService(ILocationRepository locations) => _locations = locations;

    /// <inheritdoc />
    public Task<List<LocationSummaryDto>> GetNearbyAsync(double lat, double lng, int radiusMetres = 5000)
        => _locations.GetNearbyAsync(lat, lng, radiusMetres);

    /// <inheritdoc />
    public Task<LocationDetailDto?> GetByIdAsync(Guid locationId)
        => _locations.GetByIdAsync(locationId);
}
