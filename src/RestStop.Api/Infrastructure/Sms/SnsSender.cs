namespace RestStop.Api.Infrastructure.Sms;

/// <summary>
/// Sends SMS messages via AWS Simple Notification Service.
/// Uses the transactional message type for improved deliverability.
/// </summary>
public class SnsSender : ISmsSender
{
    private readonly IAmazonSimpleNotificationService _sns;

    /// <summary>Initialises a new instance of <see cref="SnsSender"/>.</summary>
    /// <param name="sns">The AWS SNS client injected via DI.</param>
    public SnsSender(IAmazonSimpleNotificationService sns) => _sns = sns;

    /// <inheritdoc />
    public async Task SendOtpAsync(string phoneNumber, string code)
    {
        var request = new PublishRequest
        {
            PhoneNumber = phoneNumber,
            Message = $"Your RestStop code is: {code}. Expires in 10 minutes.",
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = "Transactional"
                },
                ["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = "RestStop"
                }
            }
        };

        await _sns.PublishAsync(request);
    }
}
