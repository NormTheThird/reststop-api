namespace RestStop.Api.Middleware;

/// <summary>
/// Enforces a global minimum API version and an optional per-client minimum version from JWT claims.
/// Returns <c>410 Gone</c> for sunset versions and <c>403 Forbidden</c> for client-restricted versions.
/// </summary>
public class MinimumVersionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiVersion _globalMinimumVersion;
    private readonly ILogger<MinimumVersionMiddleware> _logger;
    private readonly IApiVersionParser _versionParser;

    /// <summary>Initialises a new instance of <see cref="MinimumVersionMiddleware"/>.</summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="configuration">Application configuration — reads <c>ApiVersioning:MinimumVersion</c>.</param>
    /// <param name="logger">Logger for version enforcement actions.</param>
    /// <param name="versionParser">API version parser provided by the versioning framework.</param>
    public MinimumVersionMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<MinimumVersionMiddleware> logger,
        IApiVersionParser versionParser)
    {
        _next = next;
        _logger = logger;
        _versionParser = versionParser;

        var minVersionString = configuration["ApiVersioning:MinimumVersion"] ?? "1.0";
        _globalMinimumVersion = _versionParser.Parse(minVersionString);

        _logger.LogInformation(
            "MinimumVersionMiddleware: global minimum version is {Version}.", _globalMinimumVersion);
    }

    /// <summary>Checks the requested API version against global and per-client minimums.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var requestedVersion = context.GetRequestedApiVersion();

        if (requestedVersion is null)
        {
            await _next(context);
            return;
        }

        // Global sunset check — blocks everyone from using retired versions.
        if (requestedVersion < _globalMinimumVersion)
        {
            _logger.LogWarning(
                "Rejected sunset version {Requested} (global minimum: {Minimum}). Path: {Path}",
                requestedVersion, _globalMinimumVersion, context.Request.Path);

            await WriteJsonResponse(context, StatusCodes.Status410Gone, new
            {
                error = "API version no longer supported.",
                requestedVersion = $"v{requestedVersion}",
                minimumVersion = $"v{_globalMinimumVersion}",
                message = $"Please upgrade to v{_globalMinimumVersion} or later."
            });

            return;
        }

        // Per-client minimum check — blocks a specific client from downgrading.
        var clientMinimum = GetClientMinimumVersion(context);

        if (clientMinimum is not null && requestedVersion < clientMinimum)
        {
            var clientId = context.User?.FindFirst("client_id")?.Value ?? "unknown";

            _logger.LogWarning(
                "Client {ClientId} used version {Requested} below their minimum {Minimum}.",
                clientId, requestedVersion, clientMinimum);

            await WriteJsonResponse(context, StatusCodes.Status403Forbidden, new
            {
                error = "API version not permitted for this client.",
                requestedVersion = $"v{requestedVersion}",
                clientMinimumVersion = $"v{clientMinimum}",
                message = $"Your account requires v{clientMinimum} or later."
            });

            return;
        }

        await _next(context);
    }

    private ApiVersion? GetClientMinimumVersion(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return null;

        var claim = context.User.FindFirst("min_api_version")?.Value;

        if (string.IsNullOrWhiteSpace(claim))
            return null;

        return _versionParser.TryParse(claim, out var version) ? version : null;
    }

    private static async Task WriteJsonResponse(HttpContext context, int statusCode, object body)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(body,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
