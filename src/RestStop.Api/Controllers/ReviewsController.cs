namespace RestStop.Api.Controllers;

/// <summary>
/// Admin endpoints for moderating reviews.
/// GET and approve actions require Moderator, Admin, or SuperAdmin.
/// DELETE requires Admin or SuperAdmin.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/reviews")]
[Authorize(Roles = "Moderator,Admin,SuperAdmin")]
public class ReviewsController : BaseController
{
    private readonly AppDbContext _db;

    /// <summary>Initialises a new instance of <see cref="ReviewsController"/>.</summary>
    public ReviewsController(AppDbContext db) => _db = db;

    /// <summary>Returns paginated reviews with optional filters for flagged content or location.</summary>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Paginated review list.")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool flaggedOnly = false,
        [FromQuery] Guid? locationId = null)
    {
        var query = _db.Reviews.AsQueryable();

        if (flaggedOnly)
            query = query.Where(r => r.FlaggedCount > 0);

        if (locationId.HasValue)
            query = query.Where(r => r.Restroom.LocationId == locationId.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminReviewDto(
                r.Id,
                r.RestroomId,
                r.User.Username ?? r.User.Email ?? r.User.Phone ?? "Anonymous",
                r.Cleanliness,
                r.Smell,
                r.Supplies,
                r.Overall,
                r.PhotoAttached,
                r.HelpfulVotes,
                r.FlaggedCount,
                r.DistanceFromLocation,
                r.WeightApplied,
                r.CreatedAt))
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>Permanently deletes a review.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Review deleted.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review is null) return NotFound();

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Approves a flagged review by resetting its flag count to zero.</summary>
    [HttpPost("{id:guid}/approve")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Review approved.")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review is null) return NotFound();

        review.FlaggedCount = 0;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
