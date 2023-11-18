using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NectarineAPI.Configurations;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;

namespace NectarineAPI.Services.Messaging;

public class SendGridEmailService : IEmailService
{
    private readonly IOptions<SendGridOptions> _sendGridOptions;

    public SendGridEmailService(IOptions<SendGridOptions> sendGridOptions)
    {
        _sendGridOptions = sendGridOptions;

        Client = new SendGridClient(new SendGridClientOptions
        {
            ApiKey = _sendGridOptions.Value.ApiKey,
            HttpErrorAsException = true,
        });
    }

    private SendGridClient Client { get; }

    public async Task SendWelcomeEmail(string destinationAddress)
    {
        var htmlContent = await File.ReadAllTextAsync(Path.GetFullPath("Services/Messaging/Emails/WelcomeTemplate.html"));
        var message = new SendGridMessage
        {
            From = new EmailAddress(_sendGridOptions.Value.FromAddress, "Nectarine"),
            Subject = "Welcome to Nectarine",
            HtmlContent = htmlContent,
        };
        message.AddTo(destinationAddress);
        await TrySendEmail(message);
    }

    public async Task SendEmail(string destinationAddress, string subject, string plaintextMessageContent)
    {
        var message = new SendGridMessage
        {
            From = new EmailAddress(_sendGridOptions.Value.FromAddress, "Nectarine"),
        };
        message.AddTo(destinationAddress);
        message.Subject = subject;
        message.PlainTextContent = plaintextMessageContent;
        await TrySendEmail(message);
    }

    private async Task TrySendEmail(SendGridMessage message)
    {
        try
        {
            var response = await Client.SendEmailAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                Log.Error($"Error sending SendGrid email: {body}");
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to send email. Error: {e.Message}");
        }
    }
}
