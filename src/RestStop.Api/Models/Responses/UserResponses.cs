namespace RestStop.Api.Models.Responses;

/// <summary>A user as returned in the admin user list or detail view.</summary>
public record AdminUserDto(Guid Id, string? Email, string? Phone, string? Username, string Role, string UserType, double TrustWeight,
    int ReviewCount, bool IsOwner, bool IsActive, DateTime CreatedAt, int AccountAgeDays)
{
    public static AdminUserDto From(User u) => new(
        u.Id, u.Email, u.Phone, u.Username, u.Role.ToString(), u.UserType.ToString(),
        u.TrustWeight, u.ReviewCount, u.IsOwner, u.IsActive, u.CreatedAt, u.AccountAgeDays);
}
