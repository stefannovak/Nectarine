using Microsoft.Extensions.Options;
using Moq;
using NectarineAPI.Configurations;
using NectarineAPI.Services.Messaging;
using Xunit;

namespace NectarineTests.Services.Messaging;

public class TwilioServiceTests
{
    private readonly TwilioService _subject;

    public TwilioServiceTests()
    {
        // Twilio Options setup
        var mockTwilioOptions = new Mock<IOptions<TwilioOptions>>();

        var options = new TwilioOptions
        {
            AccountSid = "A7b",
            AuthToken = "a33",
            TwilioPhoneNumber = "1",
        };

        mockTwilioOptions
            .Setup(x => x.Value)
            .Returns(options);

        _subject = new TwilioService(mockTwilioOptions.Object);
    }

    [Fact(DisplayName = "SendMessage sends an SMS")]
    public void Test_SendMessage_SendsMessageOk()
    {
        // Act
        //TODO: - How to test this void function?
    }
}