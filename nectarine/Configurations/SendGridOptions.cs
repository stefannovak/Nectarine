using Twilio.Base;

namespace nectarineAPI.Configurations;

public class SendGridOptions
{
    public string ApiKey { get; set; } = string.Empty;
    
    public string FromAddress { get; set; } = string.Empty;
}