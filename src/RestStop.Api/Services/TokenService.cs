namespace RestStop.Api.Services;

/// <summary>
/// Handles JWT access token generation and refresh token lifecycle.
/// </summary>
public class TokenService : ITokenService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    /// <summary>Initialises a new instance of <see cref="TokenService"/>.</summary>
    /// <param name="db">Database context for persisting refresh tokens.</param>
    /// <param name="config">Application configuration for JWT settings.</param>
    public TokenService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <inheritdoc />
    public string GenerateAccessToken(User user)
    {
        var secret = _config["Jwt:Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = _config.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiryDays = _config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 30);

        var record = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays)
        };

        _db.RefreshTokens.Add(record);
        await _db.SaveChangesAsync();

        return token;
    }

    /// <inheritdoc />
    public async Task<User> ValidateRefreshTokenAsync(string token)
    {
        var record = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token);

        if (record is null || !record.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        return record.User;
    }

    /// <inheritdoc />
    public async Task RevokeRefreshTokenAsync(string token)
    {
        var record = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token);

        if (record is not null)
        {
            record.Revoked = true;
            await _db.SaveChangesAsync();
        }
    }
}
