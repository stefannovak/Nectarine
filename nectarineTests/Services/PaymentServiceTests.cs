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

        [Fact(DisplayName = "AddCardPaymentMethod should add a reference for a card to the user.")]
        public async Task Test_AddCardPaymentMethod()
        {
            // Arrange
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
        
        [Theory]
        [InlineData("4242", "9", "2025", "552")] // Invalid card number
        [InlineData("4242424242424242", "99", "2025", "552")] // Invalid month
        [InlineData("4242424242424242", "9", "9999", "552")] // Invalid year
        [InlineData("4242424242424242", "9", "2025", "0")] // Invalid CSV
        public async Task Test_AddCardPaymentMethod_ShouldFailWithInvalidCardDetails(params object[] cardData)
        {
            // Arrange
            await _userCustomerService.AddStripeCustomerIdAsync(user);
            
            // Act
            var exception = paymentService.AddCardPaymentMethod(
                user, 
                cardData[0] as string ?? "",
                int.Parse(cardData[1] as string ?? ""),
                int.Parse(cardData[2] as string ?? ""),
                cardData[3] as string ?? "");
            
            Assert.NotNull(exception);
        }

        [Fact(DisplayName = "GetCardsForUser should return a list of cards attached to the user")]
        public async Task Test_GetCardsForUser()
        {
            // Arrange
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
            // Arrange
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

        [Fact(DisplayName = "ConfirmPaymentIntent should confirm a PaymentIntent with a given client secret")]
        public async Task Test_ConfirmPaymentIntent()
        {
            // Arrange
            await _userCustomerService.AddStripeCustomerIdAsync(user);
            
            paymentService.AddCardPaymentMethod(
                user,
                "4242424242424242",
                9,
                2025,
                "552");
            var cards = paymentService.GetCardsForUser(user);
            var paymentIntent = paymentService.CreatePaymentIntent(user, 500, cards.Last().Id);

            // Act
            var newPaymentIntent = paymentService.ConfirmPaymentIntent(paymentIntent.Id);
            
            // Assert
            Assert.True(newPaymentIntent.Status == "succeeded");
        }

    }
}
