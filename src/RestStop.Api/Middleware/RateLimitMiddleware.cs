namespace RestStop.Api.Middleware;

/// <summary>
/// Rate limiting middleware. Limits requests per client IP using a sliding window counter.
/// Applies to all <c>/api/auth/send-code</c> requests to prevent OTP abuse.
/// </summary>
[ExcludeFromCodeCoverage]
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly ILogger<RateLimitMiddleware> _logger;

    private static readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _requestLogs = new();

    /// <summary>Initialises a new instance of <see cref="RateLimitMiddleware"/>.</summary>
    public RateLimitMiddleware(RequestDelegate next, IConfiguration config, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _config = config;
        _logger = logger;
    }

    /// <summary>Invokes the middleware, applying rate limiting to OTP send-code requests.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (context.Request.Path.StartsWithSegments("/api/auth/send-code"))
            {
                var maxRequests = _config.GetValue<int>("RateLimit:MaxRequestsPerWindow", 3);
                var windowSeconds = _config.GetValue<int>("RateLimit:WindowSeconds", 3600);

                var clientKey = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";

                var currentTime = DateTime.UtcNow;
                var queue = _requestLogs.GetOrAdd(clientKey, _ => new ConcurrentQueue<DateTime>());

                var exceeded = ProcessRateLimiting(queue, currentTime, maxRequests, windowSeconds);

                if (exceeded)
                {
                    _logger.LogWarning("Rate limit exceeded for client {ClientKey}.", clientKey);

                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        context.Response.Headers["Retry-After"] = windowSeconds.ToString();
                        await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                    }

                    return;
                }

                _logger.LogInformation(
                    "Request allowed for client {ClientKey}. Window count: {Count}.",
                    clientKey, queue.Count);
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RateLimitMiddleware.");
            throw;
        }
    }

    private static bool ProcessRateLimiting(
        ConcurrentQueue<DateTime> queue,
        DateTime currentTime,
        int maxRequests,
        int windowSeconds)
    {
        lock (queue)
        {
            while (queue.TryPeek(out var oldest) &&
                   (currentTime - oldest).TotalSeconds > windowSeconds)
            {
                queue.TryDequeue(out _);
            }

            if (queue.Count >= maxRequests)
                return true;

            queue.Enqueue(currentTime);
            return false;
        }
    }
}
