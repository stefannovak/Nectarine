using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Controllers;
using NectarineAPI.DTOs.Requests.Orders;
using NectarineAPI.Models.Customers;
using NectarineAPI.Models.Payments;
using NectarineAPI.Services;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using SendGrid.Helpers.Mail;
using Stripe;

namespace NectarineTests.Controllers.OrderControllerTests;

public partial class OrderControllerTests
{
    private readonly OrderController _subject;

    private readonly ApplicationUser user;
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly NectarineDbContext _context;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IUserCustomerService> _userCustomerServiceMock;

    private readonly Guid orderId = Guid.NewGuid();
    private readonly Guid addressId = Guid.NewGuid();
    private CreateOrderDTO createOrderDto;

    public OrderControllerTests()
    {
        // User setup
        const string stripeCustomerId = "customerId";
        user = new ApplicationUser
        {
            Email = "test@email.com",
            FirstName = "First Name",
            Id = Guid.NewGuid().ToString(),
            PaymentProviderCustomerId = stripeCustomerId,
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
            AddressId = addressId,
        };

        // UserManager setup
        _userManager = MockHelpers.MockUserManager<ApplicationUser>();

        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // DbContext setup
        // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-6.0/breaking-changes#in-memory-required
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb", b => b.EnableNullChecks(false))
            .EnableSensitiveDataLogging()
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
            .Returns(new SensitivePaymentMethod("pm_something", stripeCustomerId, 12, 2025, "1234"));
        
        // IUserCustomerService setup
        _userCustomerServiceMock = new Mock<IUserCustomerService>();

        _userCustomerServiceMock
            .Setup(x => x.GetCustomer(It.IsAny<string>()))
            .Returns(new UserCustomerDetails(
                "cus_123",
                "pay_123",
                "test@me.com",
                "123123123123",
                "me",
                123,
                new UserAddress
                (
                    "21 BoolProp Lane",
                    null,
                    "Big City",
                    "11111",
                    "UK",
                    true
                ))
            );

        // PaymentController setup
        _subject = new OrderController(
            _context,
            _userManager.Object,
            mapperMock.Object,
            emailServiceMock.Object,
            _paymentServiceMock.Object,
            _userCustomerServiceMock.Object);
    }
}