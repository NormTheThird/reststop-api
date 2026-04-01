namespace RestStop.Api.Interfaces.Repositories;

/// <summary>
/// Defines data access operations for the Users table.
/// </summary>
public interface IUserRepository
{
    /// <summary>Finds a user by their email address.</summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Finds a user by their phone number.</summary>
    /// <param name="phone">The E.164 phone number to search for.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByPhoneAsync(string phone);

    /// <summary>Finds a user by their unique identifier.</summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>Finds a user by their Google OAuth subject identifier.</summary>
    /// <param name="googleId">The Google subject identifier.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByGoogleIdAsync(string googleId);

    /// <summary>Finds a user by their Apple OAuth subject identifier.</summary>
    /// <param name="appleId">The Apple subject identifier.</param>
    /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
    Task<User?> GetByAppleIdAsync(string appleId);

    /// <summary>Persists a new user record to the database.</summary>
    /// <param name="user">The user to create.</param>
    /// <returns>The created <see cref="User"/> with any database-generated values populated.</returns>
    Task<User> CreateAsync(User user);

    /// <summary>Persists changes to an existing user record.</summary>
    /// <param name="user">The user entity with updated values.</param>
    /// <returns>A task representing the async update.</returns>
    Task UpdateAsync(User user);
}
