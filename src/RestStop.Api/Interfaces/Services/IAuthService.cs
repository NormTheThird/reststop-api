namespace RestStop.Api.Interfaces.Services;

/// <summary>
/// Defines authentication operations including OTP, OAuth, and password-based flows.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Sends a one-time password code to the given email address or phone number.
    /// Rate-limited to prevent abuse.
    /// </summary>
    /// <param name="recipient">An email address or E.164 phone number.</param>
    /// <returns>A task representing the async send operation.</returns>
    Task SendOtpAsync(string recipient);

    /// <summary>
    /// Verifies an OTP code and returns auth tokens if valid.
    /// Creates a new user record if this is the first sign-in for this recipient.
    /// </summary>
    /// <param name="request">The recipient and code to verify.</param>
    /// <returns>An <see cref="AuthResponse"/> containing access and refresh tokens.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the code is invalid or expired.</exception>
    Task<AuthResponse> VerifyOtpAsync(VerifyCodeRequest request);

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>An <see cref="AuthResponse"/> on success.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if credentials are invalid.</exception>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Creates a new full account with username, email, and password.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <returns>An <see cref="AuthResponse"/> for the newly created user.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the email is already registered.</exception>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Issues a new access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token string.</param>
    /// <returns>A new <see cref="AuthResponse"/> with a fresh access token.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the refresh token is invalid, expired, or revoked.</exception>
    Task<AuthResponse> RefreshAsync(string refreshToken);

    /// <summary>
    /// Revokes a refresh token, signing the user out on this device.
    /// </summary>
    /// <param name="refreshToken">The refresh token string to revoke.</param>
    /// <returns>A task representing the async revocation.</returns>
    Task LogoutAsync(string refreshToken);
}
