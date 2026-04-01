namespace RestStop.Api.Data.Repositories;

/// <summary>
/// Concrete EF Core implementation of <see cref="IUserRepository"/>.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    /// <summary>Initialises a new instance of <see cref="UserRepository"/>.</summary>
    /// <param name="db">The database context.</param>
    public UserRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public Task<User?> GetByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    /// <inheritdoc />
    public Task<User?> GetByPhoneAsync(string phone) =>
        _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);

    /// <inheritdoc />
    public Task<User?> GetByIdAsync(Guid id) =>
        _db.Users.FindAsync(id).AsTask();

    /// <inheritdoc />
    public Task<User?> GetByGoogleIdAsync(string googleId) =>
        _db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);

    /// <inheritdoc />
    public Task<User?> GetByAppleIdAsync(string appleId) =>
        _db.Users.FirstOrDefaultAsync(u => u.AppleId == appleId);

    /// <inheritdoc />
    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }
}
