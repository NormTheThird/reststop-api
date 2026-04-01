namespace RestStop.Api.Interfaces.Services;

/// <summary>
/// Defines the contract for sending OTP codes and magic links via email.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an OTP code to the specified email address.
    /// </summary>
    /// <param name="toAddress">The recipient email address.</param>
    /// <param name="code">The plain-text OTP code to include in the email.</param>
    /// <returns>A task representing the async send operation.</returns>
    Task SendOtpAsync(string toAddress, string code);
}
