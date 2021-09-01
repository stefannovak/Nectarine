using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Moq;
using nectarineAPI.Services;
using Stripe;
using Xunit;

namespace nectarineTests.Services
{
    public class StripeServiceTests
    {
        private readonly Mock<IConfigurationSection> _configurationSection = new();
        private readonly Mock<PaymentService> _stripeService = new();

        public StripeServiceTests()
        {
            _configurationSection.Setup(x => x.Path).Returns("Stripe");
            _configurationSection.Setup(x => x.Key).Returns("Secret");
            _configurationSection.Setup(x => x.Value).Returns("");
            StripeConfiguration.ApiKey = _configurationSection.Object.Value;
        }
        
        
        [Fact(DisplayName = "An API key should be correctly set up at 'Stripe:Secret' in IConfiguration")]
        public void Test_Configuration_ReturnsAnApiKey()
        {
            // Act
            var apiKey = StripeConfiguration.ApiKey;
            
            // Assert
            Assert.NotNull(apiKey);
        }

        [Fact(DisplayName = "Create the example PaymentIntent as described in https://stripe.com/docs/development/quickstart#test-api-request")]
        public void Test_CreateATestPaymentIntent()
        {
            // Arrange
            var options = new PaymentIntentCreateOptions
            {
                Amount = 1000,
                Currency = "gbp",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                ReceiptEmail = "jenny.rosen@example.com",
            };

            var service = new PaymentIntentService();
            
            // Act
            var paymentIntent = service.Create(options);

            // Assert
            Assert.NotNull(paymentIntent);
        }

        [Fact(DisplayName = "TestStripeMethod should return true")]
        public void Test_TestStripeMethod()
        {
            // Act
            var result = _stripeService.Object.TestStripeMethod();
            
            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "AddCardToAccount should add a reference for a card to the user.")]
        public void Test_AddCardToAccount()
        {
        }
    }
}