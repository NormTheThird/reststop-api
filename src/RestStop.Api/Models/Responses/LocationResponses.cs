namespace RestStop.Api.Models.Responses;

/// <summary>A lightweight summary of a location returned in map/list views.</summary>
public record LocationSummaryDto(Guid Id, string Name, string? Brand, double Lat, double Lng, double DistanceMetres, double AvgOverall,
    int TotalReviews, bool Is24Hr);

/// <summary>Full detail for a single location including individual restroom breakdowns.</summary>
public record LocationDetailDto(Guid Id, string Name, string Address, string? Brand, double Lat, double Lng, bool Is24Hr,
    string? HoursOpen, bool ClaimedByOwner, List<RestroomSummaryDto> Restrooms);

/// <summary>Rating summary for a single restroom within a location.</summary>
public record RestroomSummaryDto(Guid Id, string Type, double AvgCleanliness, double AvgSmell, double AvgSupplies, double AvgOverall,
    int ReviewCount, bool HasBabyStation, bool HasShower, bool IsAccessible);
