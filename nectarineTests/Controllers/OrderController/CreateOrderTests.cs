using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace NectarineTests.Controllers.OrderController;

public partial class OrderControllerTests
{
    [Fact(DisplayName = "CreateOrderTests should return Ok")]
    public async Task Test_CreateOrder_ReturnsOk()
    {
        // Act
        var result = await _subject.CreateOrder(createOrderDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "CreateOrderTests should return Unauthorized when a User can't be found")]
    public async Task Test_CreateOrder_FailsWhen_UserCantBeFound()
    {
        // Arrange
        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _subject.CreateOrder(createOrderDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}