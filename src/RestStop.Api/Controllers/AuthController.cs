namespace RestStop.Api.Controllers;

/// <summary>
/// Handles all authentication flows: OTP, password login, registration, and token refresh.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _auth;

    /// <summary>Initialises a new instance of <see cref="AuthController"/>.</summary>
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Sends a one-time password code to an email address or phone number.</summary>
    /// <param name="request">The recipient email or phone.</param>
    /// <returns>200 on success. 400 if the recipient format is invalid.</returns>
    [HttpPost("send-code")]
    [SwaggerResponse(StatusCodes.Status200OK, "OTP code sent successfully.")]
    public async Task<IActionResult> SendCode([FromBody] SendCodeRequest request)
    {
        await _auth.SendOtpAsync(request.Recipient);
        return Ok(new { message = "Code sent." });
    }

    /// <summary>Verifies an OTP code and returns auth tokens. Creates an account if first sign-in.</summary>
    /// <param name="request">The recipient and 6-digit code.</param>
    /// <returns>200 with <see cref="AuthResponse"/>. 401 if code is invalid or expired.</returns>
    [HttpPost("verify-code")]
    [SwaggerResponse(StatusCodes.Status200OK, "OTP verified. Returns access and refresh tokens.", typeof(AuthResponse))]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        var response = await _auth.VerifyOtpAsync(request);
        return Ok(response);
    }

    /// <summary>Authenticates a user with email and password.</summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>200 with <see cref="AuthResponse"/>. 401 if credentials are invalid.</returns>
    [HttpPost("login")]
    [SwaggerResponse(StatusCodes.Status200OK, "Login successful. Returns access and refresh tokens.", typeof(AuthResponse))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _auth.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>Creates a new full account with username, email, and password.</summary>
    /// <param name="request">The registration details.</param>
    /// <returns>201 with <see cref="AuthResponse"/>. 409 if the email is already registered.</returns>
    [HttpPost("register")]
    [SwaggerResponse(StatusCodes.Status201Created, "Account created. Returns access and refresh tokens.", typeof(AuthResponse))]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _auth.RegisterAsync(request);
        return StatusCode(201, response);
    }

    /// <summary>Exchanges a valid refresh token for a new access token.</summary>
    /// <param name="request">The refresh token.</param>
    /// <returns>200 with a new <see cref="AuthResponse"/>. 401 if the token is invalid.</returns>
    [HttpPost("refresh")]
    [SwaggerResponse(StatusCodes.Status200OK, "Token refreshed successfully.", typeof(AuthResponse))]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var response = await _auth.RefreshAsync(request.RefreshToken);
        return Ok(response);
    }

    /// <summary>Revokes the provided refresh token, signing the user out on this device.</summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <returns>204 on success.</returns>
    [Authorize]
    [HttpPost("logout")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Logged out successfully.")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _auth.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}
