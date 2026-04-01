namespace RestStop.Api.Controllers;

/// <summary>
/// Base controller that all API controllers inherit from.
/// Declares common Swagger response documentation shared across every endpoint.
/// </summary>
[SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized. Missing or invalid authentication credentials.")]
[SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden. You do not have permission to access this resource.")]
[SwaggerResponse(StatusCodes.Status404NotFound, "Not Found. The requested resource does not exist.")]
[SwaggerResponse(StatusCodes.Status405MethodNotAllowed, "Method Not Allowed. The HTTP method is not supported for this endpoint.")]
[SwaggerResponse(StatusCodes.Status429TooManyRequests, "Too Many Requests. You have exceeded the rate limit.")]
[SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error — please contact support.")]
[SwaggerResponse(StatusCodes.Status503ServiceUnavailable, "Service Unavailable — server is temporarily unable to handle the request.")]
public abstract class BaseController : ControllerBase
{
}
