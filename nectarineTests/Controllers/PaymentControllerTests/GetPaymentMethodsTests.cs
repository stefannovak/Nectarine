using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineAPI.Models.Payments;
using Xunit;

namespace NectarineTests.Controllers.PaymentControllerTests;

public partial class PaymentControllerTests
{
    [Fact(DisplayName = "GetPaymentMethods should return Unauthorized")]
    public async Task Test_GetPaymentMethods_ReturnsUnauthorized()
    {
        // Arrange
        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()));
        
        // Act
        var result = await _subject.GetPaymentMethods();
        
        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }    
        
    [Fact(DisplayName = "GetPaymentMethods should return NotFound")]
    public async Task Test_GetPaymentMethods_ReturnsNotFound()
    {
        // Arrange
        _paymentServiceMock
            .Setup(x => x.GetCardsForUser(It.IsAny<string>()))
            .Returns(new List<InsensitivePaymentCard>());
        
        // Act
        var result = await _subject.GetPaymentMethods();
        
        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
    
    [Fact(DisplayName = "GetPaymentMethods should return a list of InsensitivePaymentCard's")]
    public async Task Test_GetPaymentMethods()
    {
        // Act
        var result = await _subject.GetPaymentMethods();
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}