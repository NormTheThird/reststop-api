namespace RestStop.Api.Models;

/// <summary>
/// Represents a registered user of the RestStop platform.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public string? AppleId { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public UserType UserType { get; set; } = UserType.Unknown;
    public double TrustWeight { get; set; } = 1.0;
    public int ReviewCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsOwner { get; set; } = false;
    public DateTime CreatedAt { get; set; } = new();
    public int AccountAgeDays => (int)(DateTime.UtcNow - CreatedAt).TotalDays;
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
