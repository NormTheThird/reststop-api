namespace RestStop.Api.Models.Responses;

/// <summary>Response returned on successful authentication containing tokens and basic user info.</summary>
public record AuthResponse(string AccessToken, string RefreshToken, Guid UserId, string Role);
