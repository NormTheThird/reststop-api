namespace RestStop.Api.Enumerations;

/// <summary>Defines the access roles available in the system.</summary>
public enum UserRole
{
    /// <summary>Standard app user — no dashboard access.</summary>
    User,
    /// <summary>Can view and moderate flagged content in the admin dashboard.</summary>
    Moderator,
    /// <summary>Full admin dashboard access.</summary>
    Admin,
    /// <summary>Unrestricted access including user management.</summary>
    SuperAdmin
}
