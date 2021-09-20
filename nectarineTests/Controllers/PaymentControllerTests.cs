using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using nectarineAPI.Controllers;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.Services;
using nectarineData.DataAccess;
using nectarineData.Models;
using Stripe;
using Xunit;

namespace nectarineTests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly Mock<IConfigurationSection> _configurationSection = new ();
        private readonly PaymentController _controller;
        private readonly PaymentService _paymentService = new ();
        private readonly UserCustomerService _userCustomerService;
        private readonly ApplicationUser user = new();

        public PaymentControllerTests()
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
            mockContext.Users.Add(user);
            
            // PaymentController setup
            _controller = new PaymentController(mockContext, _paymentService);
        }

        [Fact(DisplayName = "AddPaymentMethod should add a payment method to the user")]
        public async Task Test_AddPaymentMethod()
        {
            // Arrange
            await _userCustomerService.AddStripeCustomerIdAsync(user);
            
            var addPaymentMethodDto = new AddPaymentMethodDto
            {
                CardNumber = "4242424242424242",
                ExpiryMonth = 9, 
                ExpiryYear = 2025,
                CVC = "552"
            };
            
            // Act
            _controller.AddPaymentMethod(user.Id, addPaymentMethodDto);
            var cards = _paymentService.GetCardsForUser(user);
            
            // Assert
            Assert.True(cards.Any());
        }

        [Fact(DisplayName = "AddPaymentMethod should return a NotFound when given a userId that is not in the database")]
        public async Task Test_AddPaymentMethod_ReturnsNotFoundWhenInvalidUserId()
        {
            // Arrange
            await _userCustomerService.AddStripeCustomerIdAsync(user);
            
            var addPaymentMethodDto = new AddPaymentMethodDto
            {
                CardNumber = "4242424242424242",
                ExpiryMonth = 9, 
                ExpiryYear = 2025,
                CVC = "552"
            };
            
            // Act
            var result = _controller.AddPaymentMethod("", addPaymentMethodDto);
            
            // Assert
            Assert.True(result.GetType() == typeof(NotFoundObjectResult));
        }
        
        [Fact(DisplayName = "AddPaymentMethod should return a BadRequest when passed invalid card details")]
        public async Task Test_AddPaymentMethod_ReturnsBadRequestWhenInvalidCardDetails()
        {
            // Arrange
            await _userCustomerService.AddStripeCustomerIdAsync(user);

            var addPaymentMethodDto = new AddPaymentMethodDto
            {
                CardNumber = "4242424242424242",
                ExpiryMonth = 9,
                ExpiryYear = 9999,
                CVC = "552"
            };

            // Act
            var result = _controller.AddPaymentMethod(user.Id, addPaymentMethodDto);
            
            // Assert
            Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        }
    }
}