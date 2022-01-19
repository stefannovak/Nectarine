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
                Id = Guid.NewGuid().ToString(),
                StripeCustomerId = stripeCustomerId,
            };

            // IPaymentServiceMock setup
            _paymentServiceMock = new Mock<IPaymentService>();

            _paymentServiceMock
                .Setup(x => x.AddCardPaymentMethod(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()));

            _paymentServiceMock
                .Setup(x => x.GetPaymentMethod(It.IsAny<string>()))
                .Returns(new PaymentMethod
                {
                    CustomerId = stripeCustomerId,
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
    }
}