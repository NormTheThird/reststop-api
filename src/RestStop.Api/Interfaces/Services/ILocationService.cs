namespace RestStop.Api.Interfaces.Services;

/// <summary>
/// Defines operations for discovering and retrieving gas station locations.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Returns a list of active stations within the specified radius of the given coordinates.
    /// </summary>
    /// <param name="lat">Latitude of the user's current position.</param>
    /// <param name="lng">Longitude of the user's current position.</param>
    /// <param name="radiusMetres">Search radius in metres. Defaults to 5000.</param>
    /// <returns>Stations ordered by distance ascending. Empty list if none found.</returns>
    Task<List<LocationSummaryDto>> GetNearbyAsync(double lat, double lng, int radiusMetres = 5000);

    /// <summary>
    /// Returns the full detail for a single station including all restroom ratings.
    /// </summary>
    /// <param name="locationId">The unique identifier of the station.</param>
    /// <returns>A <see cref="LocationDetailDto"/> if found, or <c>null</c> if not found or inactive.</returns>
    Task<LocationDetailDto?> GetByIdAsync(Guid locationId);
}
