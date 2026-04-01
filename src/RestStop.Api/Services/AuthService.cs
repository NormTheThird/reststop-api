namespace RestStop.Api.Services;

/// <summary>
/// Orchestrates all authentication flows: OTP, password, OAuth, and token refresh.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IOtpService _otp;
    private readonly ITokenService _tokens;
    private readonly IEmailSender _email;
    private readonly ISmsSender _sms;

    /// <summary>Initialises a new instance of <see cref="AuthService"/>.</summary>
    /// <param name="users">Repository for user data access.</param>
    /// <param name="otp">Service for generating and validating OTP codes.</param>
    /// <param name="tokens">Service for generating and validating JWT tokens.</param>
    /// <param name="email">Sender for OTP codes delivered via email.</param>
    /// <param name="sms">Sender for OTP codes delivered via SMS.</param>
    public AuthService(
        IUserRepository users,
        IOtpService otp,
        ITokenService tokens,
        IEmailSender email,
        ISmsSender sms)
    {
        _users = users;
        _otp = otp;
        _tokens = tokens;
        _email = email;
        _sms = sms;
    }

    /// <inheritdoc />
    public async Task SendOtpAsync(string recipient)
    {
        var code = await _otp.GenerateAsync(recipient);
        var isPhone = recipient.StartsWith('+');

        if (isPhone)
            await _sms.SendOtpAsync(recipient, code);
        else
            await _email.SendOtpAsync(recipient, code);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> VerifyOtpAsync(VerifyCodeRequest request)
    {
        var valid = await _otp.ValidateAsync(request.Recipient, request.Code);
        if (!valid)
            throw new UnauthorizedAccessException("Invalid or expired code.");

        var isPhone = request.Recipient.StartsWith('+');

        var user = isPhone
            ? await _users.GetByPhoneAsync(request.Recipient)
            : await _users.GetByEmailAsync(request.Recipient);

        if (user is null)
        {
            user = new User();
            if (isPhone) user.Phone = request.Recipient;
            else user.Email = request.Recipient;
            user = await _users.CreateAsync(user);
        }

        return await BuildAuthResponseAsync(user);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email);

        if (user is null || user.PasswordHash is null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated.");

        return await BuildAuthResponseAsync(user);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _users.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await _users.CreateAsync(user);
        return await BuildAuthResponseAsync(created);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var user = await _tokens.ValidateRefreshTokenAsync(refreshToken);
        await _tokens.RevokeRefreshTokenAsync(refreshToken);
        return await BuildAuthResponseAsync(user);
    }

    /// <inheritdoc />
    public Task LogoutAsync(string refreshToken)
        => _tokens.RevokeRefreshTokenAsync(refreshToken);

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var accessToken = _tokens.GenerateAccessToken(user);
        var refreshToken = await _tokens.GenerateRefreshTokenAsync(user.Id);
        return new AuthResponse(accessToken, refreshToken, user.Id, user.Role.ToString());
    }
}
