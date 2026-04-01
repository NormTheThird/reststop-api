namespace RestStop.Api.Interfaces.Services;

/// <summary>
/// Defines operations for submitting and retrieving restroom reviews.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Submits a new review for a restroom after validating GPS proximity.
    /// Updates the restroom's weighted average scores after submission.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user submitting the review.</param>
    /// <param name="request">The review content and GPS coordinates at submission time.</param>
    /// <returns>The created <see cref="ReviewDto"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the user is too far from the location.</exception>
    Task<ReviewDto> SubmitAsync(Guid userId, SubmitReviewRequest request);
}
