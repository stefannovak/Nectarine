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
    [Fact(DisplayName = "CancelOrder should return a list of Orders for the User")]
    public async Task Test_CancelOrder_ReturnsOk()
    {
        // Assert
        _context.Orders.Add(new Order
        {
            Id = orderId,
            User = user,
        });

        await _context.SaveChangesAsync();

        // Act
        var result = await _subject.CancelOrder(orderId.ToString());

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact(DisplayName = "CancelOrder should return Unauthorized when a User can't be found")]
    public async Task Test_CancelOrder_FailsWhen_UserCantBeFound()
    {
        // Arrange
        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _subject.CancelOrder(orderId.ToString());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact(DisplayName = "CancelOrder should return NotFound when an Order can't be found for a User")]
    public async Task Test_CancelOrder_FailsWhen_AnOrderCantBeFound()
    {
        // Act
        var result = await _subject.CancelOrder(orderId.ToString());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}