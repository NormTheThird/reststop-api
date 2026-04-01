namespace RestStop.Api.Interfaces.Repositories;

/// <summary>
/// Defines data access operations for the Locations table.
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Queries for active stations within a radius using PostGIS spatial indexing.
    /// </summary>
    /// <param name="lat">Latitude of the search origin.</param>
    /// <param name="lng">Longitude of the search origin.</param>
    /// <param name="radiusMetres">Radius in metres.</param>
    /// <returns>Locations ordered by distance ascending.</returns>
    Task<List<LocationSummaryDto>> GetNearbyAsync(double lat, double lng, int radiusMetres);

    /// <summary>
    /// Retrieves a single location with its full restroom breakdown.
    /// </summary>
    /// <param name="locationId">Primary key of the location record.</param>
    /// <returns>The matching <see cref="LocationDetailDto"/>, or <c>null</c> if not found or inactive.</returns>
    Task<LocationDetailDto?> GetByIdAsync(Guid locationId);
}
