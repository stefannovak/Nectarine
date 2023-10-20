using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineData.Models;
using Xunit;

namespace NectarineTests.Controllers.OrderControllerTests;

public partial class OrderControllerTests
{
    [Fact(DisplayName = "GetAllOrders should return a list of Orders for the User")]
    public async Task Test_GetAllOrders_ReturnsOk()
    {
        // Assert
        _context.Orders.Add(new Order
        {
            Id = Guid.NewGuid(),
            User = user,
        });

        _context.Orders.Add(new Order
        {
            Id = Guid.NewGuid(),
            User = user,
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _subject.GetAllOrders();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetAllOrders should return Unauthorized when a User can't be found")]
    public async Task Test_GetAllOrders_FailsWhen_UserCantBeFound()
    {
        // Arrange
        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _subject.GetAllOrders();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}