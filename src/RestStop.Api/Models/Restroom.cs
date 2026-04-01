namespace RestStop.Api.Models;

/// <summary>
/// Represents a single restroom facility within a location.
/// One location may have multiple restroom records — one per gender type.
/// </summary>
public class Restroom
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid LocationId { get; set; } = Guid.Empty;
    public RestroomType Type { get; set; }
    public int StallCount { get; set; } = 0;
    public bool IsAccessible { get; set; } = false;
    public bool HasBabyStation { get; set; } = false;
    public bool HasShower { get; set; } = false;
    public double AvgCleanliness { get; set; } = 0.0;
    public double AvgSmell { get; set; } = 0.0;
    public double AvgSupplies { get; set; } = 0.0;
    public double AvgOverall { get; set; } = 0.0;
    public int ReviewCount { get; set; } = 0;
    public double TrustScore { get; set; } = 0.0;
    public Location Location { get; set; } = null!;
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
