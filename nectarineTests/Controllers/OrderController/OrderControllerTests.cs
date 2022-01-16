using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
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
    private readonly NectarineDbContext _context;

    private readonly Guid orderId = Guid.NewGuid();
    private readonly CreateOrderDTO createOrderDto;

    public OrderControllerTests()
    {
        // CreateOrderDto setup
        createOrderDto = new CreateOrderDTO
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

        // PaymentController setup
        _subject = new NectarineAPI.Controllers.OrderController(_context, _userManager.Object, mapperMock.Object);
    }
}