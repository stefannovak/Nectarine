using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineAPI.DTOs.Requests;
using NectarineData.Models;
using Stripe;
using Xunit;

namespace NectarineTests.Controllers.PaymentControllerTests;

public partial class PaymentControllerTests
{
        [Fact(DisplayName = "AddPaymentMethod should add a payment method to the user")]
        public async Task Test_AddPaymentMethod()
        {
            // Arrange
            var addPaymentMethodDto = new AddPaymentMethodDTO
            {
                CardNumber = "4242424242424242",
                ExpiryMonth = 9,
                ExpiryYear = 2025,
                CVC = "552",
            };

            // Act
            var result = await _subject.AddPaymentMethod(addPaymentMethodDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact(DisplayName = "AddPaymentMethod should return Unauthorized when a User can't be found")]
        public async Task Test_AddPaymentMethod_ReturnsUnauthorized()
        {
            // Arrange
            _userManager
                .Setup(manager => manager
                    .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

            var addPaymentMethodDto = new AddPaymentMethodDTO
            {
                CardNumber = "4242424242424242",
                ExpiryMonth = 9,
                ExpiryYear = 2025,
                CVC = "552",
            };

            // Act
            var result = await _subject.AddPaymentMethod(addPaymentMethodDto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact(DisplayName = "AddPaymentMethod should return a BadRequest when passed invalid card details")]
        public async Task Test_AddPaymentMethod_ReturnsBadRequest()
        {
            // Arrange
            _paymentServiceMock
                .Setup(x => x.AddCardPaymentMethod(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .Returns(new StripeException());

            var addPaymentMethodDto = new AddPaymentMethodDTO
            {
                CardNumber = "4242424242424242",
                ExpiryMonth = 9,
                ExpiryYear = 9999,
                CVC = "552",
            };

            // Act
            var result = await _subject.AddPaymentMethod(addPaymentMethodDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
}