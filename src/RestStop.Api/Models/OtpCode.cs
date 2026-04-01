namespace RestStop.Api.Models;

/// <summary>
/// Represents a one-time password code sent to a user's email or phone.
/// Codes expire after a configurable number of minutes and are invalidated after use.
/// </summary>
public class OtpCode
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = new();
    public bool Used { get; set; } = false;
    public DateTime CreatedAt { get; set; } = new();
}