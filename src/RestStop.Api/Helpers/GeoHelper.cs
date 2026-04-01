namespace RestStop.Api.Helpers;

/// <summary>
/// Provides geolocation utility methods for distance calculations and GPS validation.
/// </summary>
public static class GeoHelper
{
    private const double MaxReviewDistanceMetres = 200;

    /// <summary>
    /// Calculates the distance in metres between two coordinate pairs using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point.</param>
    /// <param name="lng1">Longitude of the first point.</param>
    /// <param name="lat2">Latitude of the second point.</param>
    /// <param name="lng2">Longitude of the second point.</param>
    /// <returns>The distance between the two points in metres.</returns>
    public static double DistanceMetres(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusMetres = 6_371_000;
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return earthRadiusMetres * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    /// <summary>
    /// Determines whether a user's submitted coordinates are close enough to a location
    /// to be considered on-site for review submission.
    /// </summary>
    /// <param name="userLat">Latitude of the user at submission time.</param>
    /// <param name="userLng">Longitude of the user at submission time.</param>
    /// <param name="locationLat">Latitude of the target location.</param>
    /// <param name="locationLng">Longitude of the target location.</param>
    /// <returns><c>true</c> if the user is within the allowed review radius.</returns>
    public static bool IsWithinReviewRange(
        double userLat, double userLng,
        double locationLat, double locationLng)
    {
        return DistanceMetres(userLat, userLng, locationLat, locationLng)
               <= MaxReviewDistanceMetres;
    }

    /// <summary>
    /// Creates a PostGIS-compatible <see cref="Point"/> from latitude and longitude.
    /// Uses SRID 4326 (WGS84) as required by PostGIS geo queries.
    /// </summary>
    /// <param name="lat">Latitude value.</param>
    /// <param name="lng">Longitude value.</param>
    /// <returns>A <see cref="Point"/> with SRID 4326.</returns>
    public static Point ToPoint(double lat, double lng) =>
        new(lng, lat) { SRID = 4326 };

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
