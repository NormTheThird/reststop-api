namespace RestStop.Api.Models;

/// <summary>
/// Represents a long-lived refresh token used to obtain new access tokens.
/// Stored server-side to allow revocation on logout.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = new();
    public bool Revoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = new();
    public User User { get; set; } = null!;
    public bool IsActive => !Revoked && ExpiresAt > DateTime.UtcNow;
}
