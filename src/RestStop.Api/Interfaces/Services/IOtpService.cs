namespace RestStop.Api.Interfaces.Services;

/// <summary>
/// Defines operations for generating and validating OTP codes.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a new 6-digit OTP code for the given recipient and persists it.
    /// Any previous unused codes for this recipient are invalidated.
    /// </summary>
    /// <param name="recipient">The email or phone number this code is for.</param>
    /// <returns>The plain-text code to be sent. Not stored — only the hash is persisted.</returns>
    Task<string> GenerateAsync(string recipient);

    /// <summary>
    /// Validates an OTP code against the stored hash for the given recipient.
    /// Marks the code as used if valid.
    /// </summary>
    /// <param name="recipient">The email or phone the code was sent to.</param>
    /// <param name="code">The plain-text code entered by the user.</param>
    /// <returns><c>true</c> if the code is valid and not expired; otherwise <c>false</c>.</returns>
    Task<bool> ValidateAsync(string recipient, string code);
}
