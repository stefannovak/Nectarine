using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.DTOs.Requests.Orders;
using NectarineAPI.Services;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using SendGrid.Helpers.Mail;
using Stripe;

namespace NectarineTests.Controllers.OrderController;

public partial class OrderControllerTests
{
    private readonly NectarineAPI.Controllers.OrderController _subject;

    private readonly ApplicationUser user;
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly NectarineDbContext _context;
    private readonly Mock<IPaymentService> _paymentServiceMock;

    private readonly Guid orderId = Guid.NewGuid();
    private readonly CreateOrderDTO createOrderDto;

    public OrderControllerTests()
    {
        // User setup
        const string stripeCustomerId = "customerId";
        user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            StripeCustomerId = stripeCustomerId,
        };

        // CreateOrderDto setup
        createOrderDto = new CreateOrderDTO
        {
            ProductIds = new List<string>
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
            },
            OrderTotal = "100.25",
            PaymentMethodId = "pm_123123213123123",
        };

        // UserManager setup
        _userManager = MockHelpers.MockUserManager<ApplicationUser>();

        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // DbContext setup
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _context = new NectarineDbContext(options);
        _context.Users.Add(user);
        _context.SaveChanges();

        // Mapper setup
        var mapperMock = new Mock<IMapper>();

        // EmailService setup
        var emailServiceMock = new Mock<IEmailService>();

        emailServiceMock
            .Setup(x => x.SendEmail(
                It.IsAny<string>(),
                It.IsAny<SendGridMessage>()));

        // IPaymentService setup
        _paymentServiceMock = new Mock<IPaymentService>();

        _paymentServiceMock
            .Setup(x => x.GetPaymentMethod(It.IsAny<string>()))
            .Returns(new PaymentMethod { CustomerId = stripeCustomerId });

        // PaymentController setup
        _subject = new NectarineAPI.Controllers.OrderController(
            _context,
            _userManager.Object,
            mapperMock.Object,
            emailServiceMock.Object,
            _paymentServiceMock.Object);
    }
}