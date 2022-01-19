using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineData.Models;
using Xunit;

namespace NectarineTests.Controllers.OrderController;

public partial class OrderControllerTests
{
    [Fact(DisplayName = "GetOrder should get an order for a user")]
    public async Task Test_GetOrder_ReturnsOk()
    {
        // Arrange
        _context.Orders.Add(new Order
        {
            Id = orderId,
            User = user,
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _subject.GetOrder(orderId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetOrder should return Unauthorized when a User can't be found")]
    public async Task Test_GetOrder_FailsWhen_UserCantBeFound()
    {
        // Arrange
        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _subject.GetOrder(orderId);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact(DisplayName = "CreateOrder should return NotFound when an Order can't be found for a User")]
    public async Task Test_GetOrder_FailsWhen_AnOrderCantBeFound()
    {
        // Act
        var result = await _subject.GetOrder(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}