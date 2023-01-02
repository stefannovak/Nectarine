using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace NectarineTests.Controllers.AddressControllerTests;

public partial class AddressControllerTests
{
    [Fact(DisplayName = "GetById should return Ok")]
    public async Task Test_GetById()
    {
        // Act
        var result = _subject.GetById(_user.UserAddresses.First().Id);
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
    
    [Fact(DisplayName = "GetById should return Unauthorized")]
    public async Task Test_GetById_ReturnsUnauthorized()
    {
        // Arrange
        _mockHelpers.UserManager_ReturnsRandomId(_userManager);
        
        // Act
        var result = _subject.GetById(_user.UserAddresses.First().Id);
        
        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
    
    [Fact(DisplayName = "GetById should return NotFound")]
    public async Task Test_GetById_ReturnsNotFound()
    {
        // Act
        var result = _subject.GetById(Guid.NewGuid());
        
        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}