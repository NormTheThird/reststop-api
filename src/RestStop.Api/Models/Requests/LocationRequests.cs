namespace RestStop.Api.Models.Requests;

/// <summary>Query parameters for the nearby locations endpoint.</summary>
public record NearbyRequest
{
    public double Lat { get; init; }
    public double Lng { get; init; }
    public int RadiusMetres { get; init; } = 5000;
}

/// <summary>Request body for creating a new location via the admin API.</summary>
public record AdminCreateLocationRequest(string Name, string Address, double Lat, double Lng, string? Brand, string PlaceType, bool Is24Hr,
    string? HoursOpen);

/// <summary>Request body for partially updating a location via the admin API.</summary>
public record AdminUpdateLocationRequest(string? Name, string? Address, double? Lat, double? Lng, string? Brand, bool? Is24Hr,
    string? HoursOpen);
