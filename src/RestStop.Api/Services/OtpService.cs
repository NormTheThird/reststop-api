namespace RestStop.Api.Services;

/// <summary>
/// Generates and validates one-time password codes.
/// Codes are stored as BCrypt hashes — the plain-text code is never persisted.
/// </summary>
public class OtpService : IOtpService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    /// <summary>Initialises a new instance of <see cref="OtpService"/>.</summary>
    /// <param name="db">Database context for persisting OTP records.</param>
    /// <param name="config">Application configuration for expiry settings.</param>
    public OtpService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <inheritdoc />
    public async Task<string> GenerateAsync(string recipient)
    {
        var existing = await _db.OtpCodes
            .Where(o => o.Recipient == recipient && !o.Used && o.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var old in existing)
            old.Used = true;

        var code = Random.Shared.Next(100_000, 999_999).ToString();
        var expiryMinutes = _config.GetValue<int>("Otp:ExpiryMinutes", 10);

        var otpRecord = new OtpCode
        {
            Recipient = recipient,
            CodeHash = BCrypt.Net.BCrypt.HashPassword(code),
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        _db.OtpCodes.Add(otpRecord);
        await _db.SaveChangesAsync();

        return code;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateAsync(string recipient, string code)
    {
        var record = await _db.OtpCodes
            .Where(o => o.Recipient == recipient && !o.Used && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (record is null)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(code, record.CodeHash))
            return false;

        record.Used = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
