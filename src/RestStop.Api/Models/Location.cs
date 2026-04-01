using NetTopologySuite.Geometries;

namespace RestStop.Api.Models;

/// <summary>
/// Represents a physical gas station or rest stop location in the database.
/// </summary>
public class Location
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Point Coordinates { get; set; } = null!;
    public PlaceType PlaceType { get; set; } = PlaceType.GasStation;
    public string? Brand { get; set; }
    public decimal? GasPrice { get; set; }
    public bool Is24Hr { get; set; } = false;
    public string? HoursOpen { get; set; }
    public bool ClaimedByOwner { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = new();
    public ICollection<Restroom> Restrooms { get; set; } = new List<Restroom>();
}
