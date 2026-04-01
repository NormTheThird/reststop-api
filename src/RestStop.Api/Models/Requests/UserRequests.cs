namespace RestStop.Api.Models.Requests;

/// <summary>Request body for creating a new user account.</summary>
public record CreateUserRequest(string Email, string Password, string? Username, string? Role, string? UserType);

/// <summary>Request body for updating a user's profile fields. Null fields are ignored.</summary>
public record UpdateUserRequest(string? Username, string? Email, string? Phone, string? Role, string? UserType, double? TrustWeight);

/// <summary>Request body for resetting a user's password.</summary>
public record ResetPasswordRequest(string NewPassword);
