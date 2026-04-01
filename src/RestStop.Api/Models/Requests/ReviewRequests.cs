namespace RestStop.Api.Models.Requests;

/// <summary>Request body for submitting a new restroom review.</summary>
public record SubmitReviewRequest(Guid RestroomId, int Cleanliness, int Smell, int Supplies, int Overall, double Lat, double Lng);
