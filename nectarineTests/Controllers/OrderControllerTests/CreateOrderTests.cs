using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineAPI.DTOs.Requests.Orders;
using Xunit;

namespace NectarineTests.Controllers.OrderControllerTests;

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

    [Fact(DisplayName = "CreateOrderTests should return BadRequest when the given PaymentMethodId does not correspond to the user")]
    public async Task Test_CreateOrder_FailsWhen_WrongPaymentMethodId()
    {
        // Arrange
        _paymentServiceMock
            .Setup(x => x.GetPaymentMethod(It.IsAny<string>()));

        // Act
        var result = await _subject.CreateOrder(createOrderDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "CreateOrderTests should return BadRequest when the given AddressId does not correspond to the users addresses")]
    public async Task Test_CreateOrder_FailsWhen_WrongAddressId()
    {        
        // Arrange
        _userCustomerServiceMock
            .Setup(x => x.GetCustomer(It.IsAny<string>()));
        
        createOrderDto = new CreateOrderDTO
        {
            ProductIds = new List<string>
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
            },
            OrderTotal = "100.25",
            PaymentMethodId = "pm_123123213123123",
            AddressId = Guid.NewGuid(),
        };

        // Act
        var result = await _subject.CreateOrder(createOrderDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}