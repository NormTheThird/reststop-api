namespace RestStop.Api.Infrastructure.Email;

/// <summary>
/// Sends transactional emails via AWS Simple Email Service.
/// The from address must be verified in the AWS SES console before use.
/// </summary>
public class SesEmailSender : IEmailSender
{
    private readonly IAmazonSimpleEmailService _ses;
    private readonly IConfiguration _config;

    /// <summary>Initialises a new instance of <see cref="SesEmailSender"/>.</summary>
    /// <param name="ses">The AWS SES client injected via DI.</param>
    /// <param name="config">Application configuration for the sender address.</param>
    public SesEmailSender(IAmazonSimpleEmailService ses, IConfiguration config)
    {
        _ses = ses;
        _config = config;
    }

    /// <inheritdoc />
    public async Task SendOtpAsync(string toAddress, string code)
    {
        var from = _config["Aws:SesFromAddress"]
            ?? throw new InvalidOperationException("Aws:SesFromAddress is not configured.");

        var request = new SendEmailRequest
        {
            Source = from,
            Destination = new Destination { ToAddresses = [toAddress] },
            Message = new Message
            {
                Subject = new Content("Your RestStop verification code"),
                Body = new Body
                {
                    Text = new Content(
                        $"Your verification code is: {code}\n\nThis code expires in 10 minutes.\n\nIf you did not request this, please ignore this email."),
                    Html = new Content(
                        $"<p>Your RestStop verification code is:</p><h2>{code}</h2><p>This code expires in 10 minutes.</p>")
                }
            }
        };

        await _ses.SendEmailAsync(request);
    }
}
