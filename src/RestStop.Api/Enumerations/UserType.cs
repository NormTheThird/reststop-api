namespace RestStop.Api.Enumerations;

/// <summary>Defines the types of users for analytics and seeding strategy.</summary>
public enum UserType
{
    /// <summary>User type has not been determined.</summary>
    Unknown,
    /// <summary>General road-tripper or occasional user.</summary>
    Traveler,
    /// <summary>Professional truck driver — seeds highway corridor data.</summary>
    Trucker,
    /// <summary>Urban driver — seeds city location data.</summary>
    CityDriver,
    /// <summary>Verified gas station owner with B2B dashboard access.</summary>
    Owner
}
