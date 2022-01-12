using Microsoft.Extensions.Options;
using nectarineAPI.Configurations;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace nectarineAPI.Services.Messaging;

public class TwilioService : IPhoneService
{
    private readonly IOptions<TwilioOptions> _twilioOptions;

    public TwilioService(IOptions<TwilioOptions> twilioOptions)
    {
        _twilioOptions = twilioOptions;
    }

    public void SendMessage(string message, string destinationNumber)
    {
        TwilioClient.Init(_twilioOptions.Value.AccountSid, _twilioOptions.Value.AuthToken);

        MessageResource.Create(
            body: message,
            from: new PhoneNumber($"+{_twilioOptions.Value.TwilioPhoneNumber}"),
            to: new PhoneNumber($"+{destinationNumber}"));
    }
}