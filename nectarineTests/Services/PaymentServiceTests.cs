using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using nectarineAPI.Services;
using nectarineData.DataAccess;
using nectarineData.Models;
using Stripe;
using Xunit;

namespace nectarineTests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IConfigurationSection> _configurationSection = new ();
        private readonly PaymentService paymentService;
        private readonly ApplicationUser user = new ();

        public PaymentServiceTests()
        {
            // Configuration setup
            _configurationSection.Setup(x => x.Path).Returns("Stripe");
            _configurationSection.Setup(x => x.Key).Returns("Secret");
            _configurationSection.Setup(x => x.Value).Returns("sk_test_26PHem9AhJZvU623DfE1x4sd");
            StripeConfiguration.ApiKey = _configurationSection.Object.Value;
            
            // PaymentService setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            
            NectarineDbContext mockContext = new (options);
            paymentService = new PaymentService(mockContext);
        }
        
        
        [Fact(DisplayName = "An API key should be correctly set up at 'Stripe:Secret' in IConfiguration")]
        public void Test_Configuration_ReturnsAnApiKey()
        {
            // Assert
            Assert.NotNull(StripeConfiguration.ApiKey);
        }
        
        # region Customers

        [Fact(DisplayName = "AddStripeCustomerIdAsync should save a StripeId to the User")]
        public async Task Test_AddStripeCustomerIdAsync()
        {
            // Act
            var result = await paymentService.AddStripeCustomerIdAsync(user);
            
            // Assert
            Assert.True(result);
            Assert.NotNull(user.StripeCustomerId);
        }

        # endregion

        [Fact(DisplayName = "AddCardToAccount should add a reference for a card to the user.")]
        public async Task Test_AddCardToAccount()
        {
            // Arrange
            await paymentService.AddStripeCustomerIdAsync(user);
            
            // Act
            var result = paymentService.AddCardPaymentMethod(
                user, 
                "4242424242424242",
                9, 
                2025,
                "552");

            // Assert
            Assert.True(result);
        }

        [Fact(DisplayName = "GetCardsForUser should return a list of cards attached to the user")]
        public async Task Test_GetCardsForUser()
        {
            // Arrange
            await paymentService.AddStripeCustomerIdAsync(user);
            
            paymentService.AddCardPaymentMethod(
                user, 
                "4242424242424242",
                9, 
                2025,
                "552");
            
            paymentService.AddCardPaymentMethod(
                user, 
                "4242424242424242",
                3, 
                2022,
                "123");
            
            // Act
            var cards = paymentService.GetCardsForUser(user);
            
            // Assert
            Assert.True(cards.Any());
        }
    }
}
