using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
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
        private readonly PaymentService paymentService = new ();
        private readonly IUserCustomerService _userCustomerService;
        private readonly ApplicationUser user = new ();

        public PaymentServiceTests()
        {
            // Configuration setup
            _configurationSection.Setup(x => x.Path).Returns("Stripe");
            _configurationSection.Setup(x => x.Key).Returns("Secret");
            _configurationSection.Setup(x => x.Value).Returns("sk_test_26PHem9AhJZvU623DfE1x4sd");
            StripeConfiguration.ApiKey = _configurationSection.Object.Value;
            
            // UserCustomerService setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            NectarineDbContext mockContext = new (options);
            _userCustomerService = new UserCustomerService(mockContext);
        }

        [Fact(DisplayName = "AddCardToAccount should add a reference for a card to the user.")]
        public async Task Test_AddCardToAccount()
        {
            // Assert
            await _userCustomerService.AddStripeCustomerIdAsync(user);
            
            // Act
            paymentService.AddCardPaymentMethod(
                user, 
                "4242424242424242",
                9, 
                2025,
                "552");

            var cards = paymentService.GetCardsForUser(user);

            // Assert
            Assert.True(cards.Any());
        }

        [Fact(DisplayName = "GetCardsForUser should return a list of cards attached to the user")]
        public async Task Test_GetCardsForUser()
        {
            // Assert
            await _userCustomerService.AddStripeCustomerIdAsync(user);
            
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
        
        [Fact(DisplayName = "CreatePaymentIntent should create a PaymentIntent and attach it to the user's Customer object")]
        public async Task Test_CreatePaymentIntent() 
        {
            // Assert
            await _userCustomerService.AddStripeCustomerIdAsync(user);
            
            // Arrange
            paymentService.AddCardPaymentMethod(
                user, 
                "4242424242424242",
                9, 
                2025,
                "552");
            var cards = paymentService.GetCardsForUser(user);
            
            // Act
            var paymentIntent = paymentService.CreatePaymentIntent(user, 500, cards.Last().Id);
            
            // Assert
            Assert.False(paymentIntent.ClientSecret.IsNullOrEmpty());
        }
    }
}
