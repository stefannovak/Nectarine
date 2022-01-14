using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using nectarineAPI.Configurations;
using nectarineAPI.Services.Messaging;
using SendGrid.Helpers.Mail;
using Xunit;

namespace nectarineTests.Services.Messaging;

public class EmailServiceTests
{
    private readonly SendGridEmailService _subject;
    
    public EmailServiceTests()
    {
        // SendGrid Options setup
        var mockSendGridOptions = new Mock<IOptions<SendGridOptions>>();

        var options = new SendGridOptions
        {
            ApiKey = "SG",
            FromAddress = "from@nectarine.com"
        };
            
        mockSendGridOptions
            .Setup(x => x.Value)
            .Returns(options);
        
        // ILogger setup
        var loggerMock = new Mock<ILogger<SendGridEmailService>>();

        _subject = new SendGridEmailService(mockSendGridOptions.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "SendWelcomeEmail should send an email")]
    public async Task Test_SendWelcomeEmail()
    {
        // Act
        await _subject.SendWelcomeEmail("a");
    }
    
    [Fact(DisplayName = "SendEmail should send an email")]
    public async Task Test_SendEmail()
    {
        // Arrange
        var message = new SendGridMessage
        {
            PlainTextContent = "Test email",
            Subject = "test subject"
        };
        
        // Act
        await _subject.SendEmail("a", message);
    }
}