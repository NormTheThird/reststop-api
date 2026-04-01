namespace RestStop.Api.Models;

/// <summary>
/// Represents a flex pass parking permit holder that must be synced to MyKastl.
/// </summary>
public class FlexPassParker
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public Guid LocationId { get; set; } = Guid.Empty;
    public string PassNumber { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public DateTime? PassExpiresAt { get; set; }
    public bool SentToMyKastl { get; set; } = false;
    public DateTime? SentToMyKastlAt { get; set; }
    public string? MyKastlParkerReference { get; set; }
    public string? LastPushError { get; set; }
    public DateTime CreatedAt { get; set; } = new();
    public User User { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
