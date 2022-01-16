using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Controllers;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Services;
using NectarineData.DataAccess;
using NectarineData.Models;
using Stripe;
using Xunit;

namespace NectarineTests.Controllers
{
    public class PaymentControllerTests
    {
        private readonly PaymentController _subject;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly ApplicationUser user = new () { Id = Guid.NewGuid().ToString() };
        private readonly Mock<UserManager<ApplicationUser>> _userManager;

        public PaymentControllerTests()
        {
            // IPaymentServiceMock setup
            _paymentServiceMock = new Mock<IPaymentService>();

            _paymentServiceMock
                .Setup(x => x.AddCardPaymentMethod(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()));

            // UserManager setup
            _userManager = MockHelpers.MockUserManager<ApplicationUser>();

            _userManager
                .Setup(manager => manager
                    .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser
                {
                    PhoneNumber = "123123123123",
                    VerificationCode = 123123,
                    VerificationCodeExpiry = DateTime.Now.AddMinutes(2),
                    PhoneNumberConfirmed = false,
                });

            // DbContext setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            NectarineDbContext mockContext = new (options);
            mockContext.Users.Add(user);
            mockContext.SaveChanges();

            // PaymentController setup
            _subject = new PaymentController(mockContext, _paymentServiceMock.Object, _userManager.Object);
        }

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
                    It.IsAny<ApplicationUser>(),
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
}