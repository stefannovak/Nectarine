using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace nectarineAPI.Services.Messaging;

public interface IEmailService
{
    public Task SendWelcomeEmail(string destinationAddress);

    public Task SendEmail(string destinationAddress, SendGridMessage message);
}