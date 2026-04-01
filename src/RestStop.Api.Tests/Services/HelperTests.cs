namespace RestStop.Api.Tests.Services;

/// <summary>Unit tests for <see cref="GeoHelper"/>.</summary>
public class GeoHelperTests
{
    /// <summary>
    /// Distance between the same point should be zero.
    /// </summary>
    [Fact]
    public void DistanceMetres_SamePoint_ReturnsZero()
    {
        var distance = GeoHelper.DistanceMetres(40.7128, -74.0060, 40.7128, -74.0060);
        distance.Should().BeApproximately(0, 0.001);
    }

    /// <summary>
    /// Distance between two known points should match the expected value within tolerance.
    /// </summary>
    [Fact]
    public void DistanceMetres_KnownPoints_ReturnsCorrectDistance()
    {
        // New York to Los Angeles — approximately 3,940 km
        var distance = GeoHelper.DistanceMetres(40.7128, -74.0060, 34.0522, -118.2437);
        distance.Should().BeInRange(3_900_000, 3_980_000);
    }

    /// <summary>
    /// A user 100m from a location should be within review range.
    /// </summary>
    [Fact]
    public void IsWithinReviewRange_Within200m_ReturnsTrue()
    {
        // Points approximately 100m apart
        var result = GeoHelper.IsWithinReviewRange(40.7128, -74.0060, 40.7137, -74.0060);
        result.Should().BeTrue();
    }

    /// <summary>
    /// A user 500m from a location should not be within review range.
    /// </summary>
    [Fact]
    public void IsWithinReviewRange_Beyond200m_ReturnsFalse()
    {
        // Points approximately 500m apart
        var result = GeoHelper.IsWithinReviewRange(40.7128, -74.0060, 40.7173, -74.0060);
        result.Should().BeFalse();
    }
}

/// <summary>Unit tests for <see cref="TrustWeightHelper"/>.</summary>
public class TrustWeightHelperTests
{
    /// <summary>
    /// A brand new account with no activity should return baseline weight of 1.0.
    /// </summary>
    [Fact]
    public void Calculate_NewAccount_ReturnsBaseline()
    {
        var weight = TrustWeightHelper.Calculate(0, 0, 0, 0);
        weight.Should().Be(1.0);
    }

    /// <summary>
    /// Many flags should reduce trust weight significantly.
    /// </summary>
    [Fact]
    public void Calculate_ManyFlags_ReducesWeight()
    {
        var weight = TrustWeightHelper.Calculate(0, 0, 0, 10);
        weight.Should().BeLessThan(1.0);
    }

    /// <summary>
    /// Weight should never drop below the minimum clamp of 0.1.
    /// </summary>
    [Fact]
    public void Calculate_ExtremeFlags_ClampsToMinimum()
    {
        var weight = TrustWeightHelper.Calculate(0, 0, 0, 100);
        weight.Should().Be(0.1);
    }

    /// <summary>
    /// Weight should never exceed the maximum clamp of 3.0.
    /// </summary>
    [Fact]
    public void Calculate_ExtremePositiveActivity_ClampsToMaximum()
    {
        var weight = TrustWeightHelper.Calculate(1000, 1000, 1000, 0);
        weight.Should().Be(3.0);
    }

    /// <summary>
    /// Weighted average update should correctly incorporate a new review.
    /// </summary>
    [Fact]
    public void UpdateWeightedAverage_SingleReview_ReturnsCorrectAverage()
    {
        var result = TrustWeightHelper.UpdateWeightedAverage(0, 0, 4, 1.0);
        result.Should().Be(4.0);
    }
}
