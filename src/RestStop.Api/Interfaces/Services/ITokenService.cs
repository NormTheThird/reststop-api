namespace RestStop.Api.Interfaces.Services;

/// <summary>
/// Defines operations for generating and validating JWT tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>A signed JWT string.</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a new opaque refresh token, persists it, and returns the token string.
    /// </summary>
    /// <param name="userId">The identifier of the user this token belongs to.</param>
    /// <returns>The opaque refresh token string to send to the client.</returns>
    Task<string> GenerateRefreshTokenAsync(Guid userId);

    /// <summary>
    /// Validates a refresh token and returns the associated user if valid.
    /// </summary>
    /// <param name="token">The refresh token string to validate.</param>
    /// <returns>The associated <see cref="User"/> if valid.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the token is invalid, expired, or revoked.</exception>
    Task<User> ValidateRefreshTokenAsync(string token);

    /// <summary>
    /// Revokes a refresh token so it can no longer be used.
    /// </summary>
    /// <param name="token">The refresh token string to revoke.</param>
    /// <returns>A task representing the async revocation.</returns>
    Task RevokeRefreshTokenAsync(string token);
}
