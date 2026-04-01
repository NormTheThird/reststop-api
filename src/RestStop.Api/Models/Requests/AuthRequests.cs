namespace RestStop.Api.Models.Requests;

/// <summary>Request to send an OTP code to an email address or phone number.</summary>
public record SendCodeRequest(string Recipient);

/// <summary>Request to verify a previously sent OTP code.</summary>
public record VerifyCodeRequest(string Recipient, string Code);

/// <summary>Request to sign in with an email address and password.</summary>
public record LoginRequest(string Email, string Password);

/// <summary>Request to create a full account with a username, email, and password.</summary>
public record RegisterRequest(string Username, string Email, string Password);

/// <summary>Request to exchange a refresh token for a new access token.</summary>
public record RefreshRequest(string RefreshToken);
