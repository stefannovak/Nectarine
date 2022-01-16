using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.DTOs.Requests.Orders;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineTests.Controllers.OrderController;

public partial class OrderControllerTests
{
    private readonly NectarineAPI.Controllers.OrderController _subject;
    private readonly ApplicationUser user = new () { Id = Guid.NewGuid().ToString() };
    private readonly Mock<UserManager<ApplicationUser>> _userManager;

    private readonly CreateOrderDto createOrderDto;

    public OrderControllerTests()
    {
        // CreateOrderDto setup
        createOrderDto = new ()
        {
            ProductIds = new List<string>
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
            },
        };

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
        _subject = new NectarineAPI.Controllers.OrderController(mockContext, _userManager.Object);
    }
}