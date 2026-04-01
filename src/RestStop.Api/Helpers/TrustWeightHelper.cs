namespace RestStop.Api.Helpers;

/// <summary>
/// Calculates trust weight multipliers applied to reviews based on user signals.
/// The weighted average on Restroom uses this weight rather than a simple mean.
/// </summary>
public static class TrustWeightHelper
{
    /// <summary>
    /// Computes a trust weight multiplier for a user based on their account history.
    /// Starts at 1.0 and increases with positive signals, decreases with negative ones.
    /// </summary>
    /// <param name="accountAgeDays">Number of days since the account was created.</param>
    /// <param name="reviewCount">Total number of reviews submitted by the user.</param>
    /// <param name="helpfulVotesReceived">Total helpful votes received across all reviews.</param>
    /// <param name="timesFlaged">Total number of times the user's reviews have been flagged.</param>
    /// <returns>
    /// A trust weight value. 1.0 is baseline. Values above 1.0 increase influence;
    /// values below 1.0 decrease it. Clamped between 0.1 and 3.0.
    /// </returns>
    public static double Calculate(
        int accountAgeDays,
        int reviewCount,
        int helpfulVotesReceived,
        int timesFlaged)
    {
        var weight = 1.0;

        weight += Math.Min(accountAgeDays / 30, 12) * 0.1;
        weight += Math.Min(reviewCount, 50) * 0.05;
        weight += Math.Min(helpfulVotesReceived, 20) * 0.1;
        weight -= timesFlaged * 0.3;

        return Math.Clamp(weight, 0.1, 3.0);
    }

    /// <summary>
    /// Recalculates the weighted average score for a restroom after a new review is added.
    /// Uses the existing aggregate values rather than recomputing from all reviews.
    /// </summary>
    /// <param name="currentAvg">The current weighted average score.</param>
    /// <param name="currentWeightedCount">The sum of all weights applied to previous reviews.</param>
    /// <param name="newScore">The score from the new review (1–5).</param>
    /// <param name="newWeight">The trust weight of the user submitting the new review.</param>
    /// <returns>The updated weighted average, rounded to two decimal places.</returns>
    public static double UpdateWeightedAverage(
        double currentAvg,
        double currentWeightedCount,
        int newScore,
        double newWeight)
    {
        var totalWeight = currentWeightedCount + newWeight;
        var newAvg = (currentAvg * currentWeightedCount + newScore * newWeight) / totalWeight;
        return Math.Round(newAvg, 2);
    }
}
