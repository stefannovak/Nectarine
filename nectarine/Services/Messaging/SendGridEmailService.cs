using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        var message = new SendGridMessage
        {
            From = new EmailAddress(_sendGridOptions.Value.FromAddress, "Nectarine"),
            Subject = "Welcome to Nectarine",
            HtmlContent = 
                """
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta http-equiv="X-UA-Compatible" content="IE=edge">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Welcome to Nectarine!</title>
                    <style>
                        body {
                            font-family: 'Arial', sans-serif;
                            background-color: #f4f4f4;
                            color: #333;
                            margin: 0;
                            padding: 20px;
                            text-align: center;
                        }
                
                        h1 {
                            color: #47a8f5;
                        }
                
                        p {
                            font-size: 16px;
                            line-height: 1.6;
                            margin-bottom: 15px;
                        }
                
                        .signature {
                            font-style: italic;
                            color: #888;
                        }
                    </style>
                </head>
                <body>
                
                    <h1>Welcome to Nectarine!</h1>
                
                    <p>You have successfully signed up to Nectarine.</p>
                    <p>This is a side project I've been working on to showcase my .NET skills.</p>
                    <p>I hope you enjoy the app!</p>
                
                    <p class="signature">Stefan Novak<br>
                    This email was sent with SendGrid.</p>
                
                </body>
                </html>
                """;
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
