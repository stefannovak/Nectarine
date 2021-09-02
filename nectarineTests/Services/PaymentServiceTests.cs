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
        private readonly ApplicationUser user;

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
            
            // User setup
            user = new ApplicationUser
            {
                StripeCustomerId = "cus_K9e3DfwxgkSfbu",
            };
        }
        
        
        [Fact(DisplayName = "An API key should be correctly set up at 'Stripe:Secret' in IConfiguration")]
        public void Test_Configuration_ReturnsAnApiKey()
        {
            // Act
            var apiKey = StripeConfiguration.ApiKey;
            
            // Assert
            Assert.NotNull(apiKey);
        }
        
        # region Customers
        
        
        
        # endregion

        [Fact(DisplayName = "AddCardToAccount should add a reference for a card to the user.")]
        public async Task Test_AddCardToAccount()
        {
            // Arrange
            var amountOfPaymentMethods = user.PaymentMethodIds.Count;
            
            // Act
            await paymentService.AddCardToAccountAsync(
                user, 
                "4242424242424242",
                9, 
                2025,
                "552");
            var result = user.PaymentMethodIds.Count;

            // Assert
            Assert.True(result == amountOfPaymentMethods + 1);
        }
        
        [Fact(DisplayName = "AddCardToAccount should fail if incorrect parameters are passed")]
        public async Task Test_AddCardToAccount_ShouldThrowException()
        {
            // Assert
            await Assert.ThrowsAsync<StripeException>(() => paymentService.AddCardToAccountAsync(
                user, 
                "42424242",
                9, 
                2025,
                "552"));
            
            await Assert.ThrowsAsync<StripeException>(() => paymentService.AddCardToAccountAsync(
                user, 
                "4242424242424242",
                123, 
                2025,
                "552"));
            
            await Assert.ThrowsAsync<StripeException>(() => paymentService.AddCardToAccountAsync(
                user, 
                "4242424242424242",
                9, 
                1,
                "552"));
            
            await Assert.ThrowsAsync<StripeException>(() => paymentService.AddCardToAccountAsync(
                user, 
                "4242424242424242",
                9, 
                2025,
                "5521231235"));
        }

        [Fact(DisplayName = "GetCardsForUser should return a list of cards attached to the user")]
        public async Task Test_GetCardsForUser()
        {
            // Arrange
            await paymentService.AddCardToAccountAsync(
                user, 
                "4242424242424242",
                9, 
                2025,
                "552");
            
            await paymentService.AddCardToAccountAsync(
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
