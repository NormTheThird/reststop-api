namespace RestStop.Api.Models;

/// <summary>
/// Represents a user-submitted rating for a specific restroom.
/// Stores GPS integrity data at submission time for auditing.
/// </summary>
public class Review
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public Guid RestroomId { get; set; } = Guid.Empty;
    public int Cleanliness { get; set; }
    public int Smell { get; set; }
    public int Supplies { get; set; }
    public int Overall { get; set; }
    public double LatAtSubmit { get; set; } = 0.0;
    public double LngAtSubmit { get; set; } = 0.0;
    public double DistanceFromLocation { get; set; } = 0.0;
    public bool PhotoAttached { get; set; } = false;
    public string? PhotoS3Key { get; set; }
    public int HelpfulVotes { get; set; } = 0;
    public int FlaggedCount { get; set; } = 0;
    public double WeightApplied { get; set; } = 1.0;
    public DateTime CreatedAt { get; set; } = new();
    public User User { get; set; } = null!;
    public Restroom Restroom { get; set; } = null!;
}
