using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Controllers;
using NectarineAPI.Models.Payments;
using NectarineAPI.Services;
using NectarineData.DataAccess;
using NectarineData.Models;
using Stripe;

namespace NectarineTests.Controllers.PaymentControllerTests
{
    public partial class PaymentControllerTests
    {
        private readonly PaymentController _subject;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly ApplicationUser user;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private string paymentMethodId = "paymentMethodId";

        public PaymentControllerTests()
        {
            // User setup
            const string stripeCustomerId = "stripeCustomerId";
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                PaymentProviderCustomerId = stripeCustomerId,
            };

            // IPaymentServiceMock setup
            _paymentServiceMock = new Mock<IPaymentService>();

            _paymentServiceMock
                .Setup(x => x.AddCardPaymentMethod(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .Returns(true);

            _paymentServiceMock
                .Setup(x => x.GetPaymentMethod(It.IsAny<string>()))
                .Returns(new SensitivePaymentMethod("pm_something", stripeCustomerId, 12, 2025, "1234"));

            _paymentServiceMock
                .Setup(x => x.GetCardsForUser(It.IsAny<string>()))
                .Returns(new List<InsensitivePaymentMethod>
                {
                    new (12, 25, "121")
                });

            // UserManager setup
            _userManager = MockHelpers.MockUserManager<ApplicationUser>();

            _userManager
                .Setup(manager => manager
                    .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser
                {
                    PhoneNumber = "123123123123",
                    VerificationCode = 123123,
                    PhoneNumberConfirmed = false,
                    PaymentProviderCustomerId = stripeCustomerId,
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
    }
}