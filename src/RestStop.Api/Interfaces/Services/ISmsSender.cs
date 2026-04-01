namespace RestStop.Api.Interfaces.Services;

/// <summary>
/// Defines the contract for sending OTP codes via SMS.
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// Sends an OTP code to the specified phone number via SMS.
    /// </summary>
    /// <param name="phoneNumber">The recipient phone number in E.164 format.</param>
    /// <param name="code">The plain-text OTP code to include in the message.</param>
    /// <returns>A task representing the async send operation.</returns>
    Task SendOtpAsync(string phoneNumber, string code);
}
