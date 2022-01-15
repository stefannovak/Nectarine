using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nectarineAPI.Configurations;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace nectarineAPI.Services.Messaging;

public class SendGridEmailService : IEmailService
{
    private readonly IOptions<SendGridOptions> _sendGridOptions;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        IOptions<SendGridOptions> sendGridOptions,
        ILogger<SendGridEmailService> logger)
    {
        _sendGridOptions = sendGridOptions;
        _logger = logger;

        Client = new SendGridClient(new SendGridClientOptions
        {
            ApiKey = _sendGridOptions.Value.ApiKey,
            HttpErrorAsException = true,
        });
    }

    private SendGridClient Client { get; }

    public async Task SendWelcomeEmail(string destinationAddress)
    {
        var message = new SendGridMessage
        {
            From = new EmailAddress(_sendGridOptions.Value.FromAddress, "Stefan Novak"),
            Subject = "Welcome to Nectarine",
            PlainTextContent =
                "Welcome to Nectarine!\n\n" +
                "You have successfully signed up to Nectarine.\n" +
                "This is a side project I've been working on to showcase my .NET skills.\n" +
                "I hope you enjoy the app!\n\n" +
                "Stefan Novak\n" +
                "This email was sent with SendGrid.",
        };
        message.AddTo(destinationAddress);
        await TrySendEmail(message);
    }

    public async Task SendEmail(string destinationAddress, SendGridMessage message)
    {
        message.From = new EmailAddress(_sendGridOptions.Value.FromAddress, "Stefan Novak");
        message.AddTo(destinationAddress);
        await TrySendEmail(message);
    }

    private async Task TrySendEmail(SendGridMessage message)
    {
        try
        {
            await Client.SendEmailAsync(message).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to send email. Error: {e}");
        }
    }
}