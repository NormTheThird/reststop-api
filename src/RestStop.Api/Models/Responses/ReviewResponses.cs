namespace RestStop.Api.Models.Responses;

/// <summary>A submitted review as returned in public list or detail responses.</summary>
public record ReviewDto(Guid Id, Guid RestroomId, string ReviewerName, int Cleanliness, int Smell, int Supplies, int Overall,
    bool PhotoAttached, int HelpfulVotes, DateTime CreatedAt);

/// <summary>A review as returned in admin views, including moderation fields.</summary>
public record AdminReviewDto(Guid Id, Guid RestroomId, string ReviewerName, int Cleanliness, int Smell, int Supplies, int Overall,
    bool PhotoAttached, int HelpfulVotes, int FlaggedCount, double DistanceFromLocation, double WeightApplied, DateTime CreatedAt);
