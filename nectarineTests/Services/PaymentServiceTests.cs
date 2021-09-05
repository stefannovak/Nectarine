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
        private readonly ApplicationUser user;

        public PaymentServiceTests()
        {
            // Configuration setup
            _configurationSection.Setup(x => x.Path).Returns("Stripe");
            _configurationSection.Setup(x => x.Key).Returns("Secret");
            _configurationSection.Setup(x => x.Value).Returns("sk_test_26PHem9AhJZvU623DfE1x4sd");
            StripeConfiguration.ApiKey = _configurationSection.Object.Value;
            
            // User setup
            user = new ApplicationUser
            {
                StripeCustomerId = "cus_KA40E33elPSaag"
            };
        }

        [Fact(DisplayName = "AddCardToAccount should add a reference for a card to the user.")]
        public async Task Test_AddCardToAccount()
        {
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
        public void Test_CreatePaymentIntent() 
        {
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
