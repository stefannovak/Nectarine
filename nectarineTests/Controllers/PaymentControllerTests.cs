using System;
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

            // DbContext setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            NectarineDbContext mockContext = new (options);
            mockContext.Users.Add(user);
            mockContext.SaveChanges();

            // PaymentController setup
            _subject = new PaymentController(mockContext, _paymentServiceMock.Object);
        }

        [Fact(DisplayName = "AddPaymentMethod should add a payment method to the user")]
        public void Test_AddPaymentMethod()
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
            var result = _subject.AddPaymentMethod(user.Id, addPaymentMethodDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact(DisplayName = "AddPaymentMethod should return a NotFound when given a userId that is not in the database")]
        public void Test_AddPaymentMethod_ReturnsNotFoundWhenInvalidUserId()
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
            var result = _subject.AddPaymentMethod(string.Empty, addPaymentMethodDto);

            // Assert
            Assert.True(result.GetType() == typeof(NotFoundObjectResult));
        }

        [Fact(DisplayName = "AddPaymentMethod should return a BadRequest when passed invalid card details")]
        public void Test_AddPaymentMethod_ReturnsBadRequestWhenInvalidCardDetails()
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
            var result = _subject.AddPaymentMethod(user.Id, addPaymentMethodDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}