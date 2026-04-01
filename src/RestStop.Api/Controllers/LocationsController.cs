namespace RestStop.Api.Controllers;

/// <summary>
/// Admin endpoints for managing gas station locations.
/// Requires Admin or SuperAdmin role.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/locations")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class LocationsController : BaseController
{
    private readonly AppDbContext _db;

    /// <summary>Initialises a new instance of <see cref="LocationsController"/>.</summary>
    public LocationsController(AppDbContext db) => _db = db;

    /// <summary>Returns all locations (active and inactive) with optional search, paginated.</summary>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Paginated list of locations.")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string search = "")
    {
        var query = _db.Locations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l =>
                l.Name.Contains(search) ||
                (l.Brand != null && l.Brand.Contains(search)) ||
                l.Address.Contains(search));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(l => l.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LocationSummaryDto(
                l.Id,
                l.Name,
                l.Brand,
                l.Coordinates.Y,
                l.Coordinates.X,
                0.0,
                l.Restrooms.Any() ? l.Restrooms.Average(r => r.AvgOverall) : 0.0,
                l.Restrooms.Sum(r => r.ReviewCount),
                l.Is24Hr))
            .ToListAsync();

        return Ok(new { items, total });
    }

    /// <summary>Returns full detail for a single location including all restroom breakdowns.</summary>
    [HttpGet("{id:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Location detail returned.", typeof(LocationDetailDto))]
    public async Task<IActionResult> GetById(Guid id)
    {
        var location = await _db.Locations
            .Include(l => l.Restrooms)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location is null) return NotFound();

        return Ok(new LocationDetailDto(
            location.Id,
            location.Name,
            location.Address,
            location.Brand,
            location.Coordinates.Y,
            location.Coordinates.X,
            location.Is24Hr,
            location.HoursOpen,
            location.ClaimedByOwner,
            location.Restrooms.Select(r => new RestroomSummaryDto(
                r.Id,
                r.Type.ToString(),
                r.AvgCleanliness,
                r.AvgSmell,
                r.AvgSupplies,
                r.AvgOverall,
                r.ReviewCount,
                r.HasBabyStation,
                r.HasShower,
                r.IsAccessible)).ToList()));
    }

    /// <summary>Creates a new location.</summary>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, "Location created.", typeof(LocationDetailDto))]
    public async Task<IActionResult> Create([FromBody] AdminCreateLocationRequest request)
    {
        if (!Enum.TryParse<PlaceType>(request.PlaceType, out var placeType))
            return BadRequest(new { message = $"Invalid PlaceType '{request.PlaceType}'." });

        var location = new Location
        {
            Name = request.Name,
            Address = request.Address,
            Coordinates = new Point(request.Lng, request.Lat) { SRID = 4326 },
            PlaceType = placeType,
            Brand = request.Brand,
            Is24Hr = request.Is24Hr,
            HoursOpen = request.HoursOpen
        };

        _db.Locations.Add(location);
        await _db.SaveChangesAsync();

        return StatusCode(201, new LocationDetailDto(
            location.Id,
            location.Name,
            location.Address,
            location.Brand,
            location.Coordinates.Y,
            location.Coordinates.X,
            location.Is24Hr,
            location.HoursOpen,
            location.ClaimedByOwner,
            new List<RestroomSummaryDto>()));
    }

    /// <summary>Partially updates an existing location.</summary>
    [HttpPatch("{id:guid}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Location updated.", typeof(LocationDetailDto))]
    public async Task<IActionResult> Update(Guid id, [FromBody] AdminUpdateLocationRequest request)
    {
        var location = await _db.Locations
            .Include(l => l.Restrooms)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location is null) return NotFound();

        if (request.Name is not null) location.Name = request.Name;
        if (request.Address is not null) location.Address = request.Address;
        if (request.Brand is not null) location.Brand = request.Brand;
        if (request.Is24Hr.HasValue) location.Is24Hr = request.Is24Hr.Value;
        if (request.HoursOpen is not null) location.HoursOpen = request.HoursOpen;

        if (request.Lat.HasValue || request.Lng.HasValue)
        {
            var lat = request.Lat ?? location.Coordinates.Y;
            var lng = request.Lng ?? location.Coordinates.X;
            location.Coordinates = new Point(lng, lat) { SRID = 4326 };
        }

        await _db.SaveChangesAsync();

        return Ok(new LocationDetailDto(
            location.Id,
            location.Name,
            location.Address,
            location.Brand,
            location.Coordinates.Y,
            location.Coordinates.X,
            location.Is24Hr,
            location.HoursOpen,
            location.ClaimedByOwner,
            location.Restrooms.Select(r => new RestroomSummaryDto(
                r.Id,
                r.Type.ToString(),
                r.AvgCleanliness,
                r.AvgSmell,
                r.AvgSupplies,
                r.AvgOverall,
                r.ReviewCount,
                r.HasBabyStation,
                r.HasShower,
                r.IsAccessible)).ToList()));
    }

    /// <summary>Soft-deletes a location by marking it inactive.</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Location deactivated.")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var location = await _db.Locations.FindAsync(id);
        if (location is null) return NotFound();

        location.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
