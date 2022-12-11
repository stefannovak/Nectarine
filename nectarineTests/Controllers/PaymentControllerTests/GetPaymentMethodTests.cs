using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineAPI.Models.Payments;
using NectarineData.Models;
using Stripe;
using Xunit;

namespace NectarineTests.Controllers.PaymentControllerTests;

public partial class PaymentControllerTests
{
    [Fact(DisplayName = "GetPaymentMethod should get an order for a user")]
    public async Task Test_GetPaymentMethod_ReturnsOk()
    {
        // Act
        var result = await _subject.GetPaymentMethod(paymentMethodId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetPaymentMethod should return Unauthorized when a User can't be found")]
    public async Task Test_GetPaymentMethod_FailsWhen_UserCantBeFound()
    {
        // Arrange
        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _subject.GetPaymentMethod(paymentMethodId);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact(DisplayName = "GetPaymentMethod should return NotFound when an PaymentMethod can't be found for a User")]
    public async Task Test_GetPaymentMethod_FailsWhen_AnOrderCantBeFound()
    {
        // Arrange
        _paymentServiceMock
            .Setup(x => x.GetPaymentMethod(It.IsAny<string>()))
            .Returns(new SensitivePaymentMethod("pm_something", "otherCustomerId", 12, 2025, "1234"));

        // Act
        var result = await _subject.GetPaymentMethod(paymentMethodId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}