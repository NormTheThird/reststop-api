namespace RestStop.Api.Controllers;

/// <summary>
/// Endpoints for managing user accounts.
/// Most actions require Admin or SuperAdmin role.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UsersController : BaseController
{
    private readonly AppDbContext _db;

    /// <summary>Initialises a new instance of <see cref="UsersController"/>.</summary>
    public UsersController(AppDbContext db) => _db = db;

    /// <summary>Returns a paginated list of users with optional search.</summary>
    /// <param name="page">The page number to return. Defaults to 1.</param>
    /// <param name="pageSize">The number of results per page. Defaults to 20.</param>
    /// <param name="search">Optional search term matched against email, phone, or username.</param>
    /// <returns>A paginated result containing matching users and total count.</returns>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Paginated user list.")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = "")
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                (u.Phone != null && u.Phone.Contains(search)) ||
                (u.Username != null && u.Username.Contains(search)));

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => AdminUserDto.From(u))
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>Returns the full detail for a single user.</summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The matching <see cref="AdminUserDto"/>, or 404 if not found.</returns>
    [HttpGet("{id:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, "User detail returned.", typeof(AdminUserDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        return Ok(AdminUserDto.From(user));
    }

    /// <summary>Creates a new user account.</summary>
    /// <param name="request">The details of the user to create.</param>
    /// <returns>The created <see cref="AdminUserDto"/> with a 201 status.</returns>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, "User created.", typeof(AdminUserDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid role or user type.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Email already in use.")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var existing = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (existing) return Conflict(new { message = "A user with that email already exists." });

        var role = UserRole.User;
        if (request.Role is not null && !Enum.TryParse(request.Role, out role))
            return BadRequest(new { message = $"Invalid role '{request.Role}'." });

        var userType = UserType.Unknown;
        if (request.UserType is not null && !Enum.TryParse(request.UserType, out userType))
            return BadRequest(new { message = $"Invalid user type '{request.UserType}'." });

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            UserType = userType
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(201, AdminUserDto.From(user));
    }

    /// <summary>Updates a user's profile fields. Only non-null fields are applied.</summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="request">The fields to update. Any null field is left unchanged.</param>
    /// <returns>The updated <see cref="AdminUserDto"/>, or 404 if not found.</returns>
    [HttpPatch("{id:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, "User updated.", typeof(AdminUserDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid role or user type.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (request.Role is not null)
        {
            if (!Enum.TryParse<UserRole>(request.Role, out var role))
                return BadRequest(new { message = $"Invalid role '{request.Role}'." });
            user.Role = role;
        }

        if (request.UserType is not null)
        {
            if (!Enum.TryParse<UserType>(request.UserType, out var userType))
                return BadRequest(new { message = $"Invalid user type '{request.UserType}'." });
            user.UserType = userType;
        }

        if (request.Username is not null) user.Username = request.Username;
        if (request.Email is not null) user.Email = request.Email;
        if (request.Phone is not null) user.Phone = request.Phone;
        if (request.TrustWeight.HasValue) user.TrustWeight = request.TrustWeight.Value;

        await _db.SaveChangesAsync();
        return Ok(AdminUserDto.From(user));
    }

    /// <summary>Permanently deletes a user account and all associated data.</summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>204 on success, or 404 if not found.</returns>
    [HttpDelete("{id:guid}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "User deleted.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Deactivates a user account and revokes all active sessions.</summary>
    /// <param name="id">The unique identifier of the user to deactivate.</param>
    /// <returns>204 on success, or 404 if not found.</returns>
    [HttpPost("{id:guid}/deactivate")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "User deactivated.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsActive = false;
        await RevokeAllSessions(id);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Reactivates a previously deactivated user account.</summary>
    /// <param name="id">The unique identifier of the user to reactivate.</param>
    /// <returns>204 on success, or 404 if not found.</returns>
    [HttpPost("{id:guid}/reactivate")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "User reactivated.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> Reactivate(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.IsActive = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Revokes all active sessions for a user without deactivating the account.</summary>
    /// <param name="id">The unique identifier of the user whose sessions will be revoked.</param>
    /// <returns>204 on success, or 404 if not found.</returns>
    [HttpPost("{id:guid}/revoke-sessions")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Sessions revoked.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> RevokeSessions(Guid id)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == id);
        if (!userExists) return NotFound();

        await RevokeAllSessions(id);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Resets a user's password to a new value.</summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="request">The new password to set.</param>
    /// <returns>204 on success, or 404 if not found.</returns>
    [HttpPost("{id:guid}/reset-password")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Password reset.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Returns the paginated review history for a user.</summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="page">The page number to return. Defaults to 1.</param>
    /// <param name="pageSize">The number of results per page. Defaults to 20.</param>
    /// <returns>A paginated result containing the user's reviews and total count.</returns>
    [HttpGet("{id:guid}/reviews")]
    [Authorize(Roles = "Moderator,Admin,SuperAdmin")]
    [SwaggerResponse(StatusCodes.Status200OK, "User review history.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> GetReviews(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == id);
        if (!userExists) return NotFound();

        var total = await _db.Reviews.CountAsync(r => r.UserId == id);

        var items = await _db.Reviews
            .Where(r => r.UserId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminReviewDto(
                r.Id,
                r.RestroomId,
                r.User.Username ?? r.User.Email ?? r.User.Phone ?? "Anonymous",
                r.Cleanliness,
                r.Smell,
                r.Supplies,
                r.Overall,
                r.PhotoAttached,
                r.HelpfulVotes,
                r.FlaggedCount,
                r.DistanceFromLocation,
                r.WeightApplied,
                r.CreatedAt))
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    private async Task RevokeAllSessions(Guid userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && !t.Revoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.Revoked = true;
    }
}
