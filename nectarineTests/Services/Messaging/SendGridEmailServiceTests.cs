using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NectarineAPI.Configurations;
using NectarineAPI.Services.Messaging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Xunit;
using IInvocation = Moq.IInvocation;

namespace NectarineTests.Services.Messaging;

public class SendGridEmailServiceTests
{
    private readonly SendGridEmailService _subject;
    private readonly Mock<SendGridClient> _clientMock;
    private readonly Mock<IOptions<SendGridOptions>> _optionsMock;

    public SendGridEmailServiceTests()
    {
        // SendGrid Options setup
        _optionsMock = new Mock<IOptions<SendGridOptions>>();

        var options = new SendGridOptions
        {
            ApiKey = "SG.12345678901234567890123456789012",
            FromAddress = "from@nectarine.com",
        };

        _optionsMock
            .Setup(x => x.Value)
            .Returns(options);

        // // SendGridClient mock
        // _clientMock = new Mock<SendGridClient>(new SendGridClientOptions
        // {
        //     ApiKey = _optionsMock.Object.Value.ApiKey,
        //     HttpErrorAsException = true,
        // });
        //
        // _clientMock.Setup(x => x.SendEmailAsync(
        //         It.IsAny<SendGridMessage>(),
        //         It.IsAny<CancellationToken>()))
        //     .Returns(Task.FromResult(new Response(System.Net.HttpStatusCode.OK, null, null)));

        _subject = new SendGridEmailService(_optionsMock.Object);
    }

    [Fact(DisplayName = "SendWelcomeEmail should send an email")]
    public async Task Test_SendWelcomeEmail()
    {
        // Arrange
        var destinationAddress = "to@example.com";

        // Act
        await _subject.SendWelcomeEmail(destinationAddress);

        // Assert
    }

    [Fact(DisplayName = "SendEmail should send an email")]
    public async Task Test_SendEmail()
    {
        // Act
        await _subject.SendEmail("a", "Test email", "test subject");
    }
}