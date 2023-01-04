using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace NectarineTests.Controllers.AddressControllerTests;

public partial class AddressControllerTests
{
    [Fact(DisplayName = "GetAll should return Ok")]
    public async Task Test_GetAll_ReturnsOk()
    {
        // Act
        var result = _subject.GetAll();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetAll should return Unauthorized")]
    public async Task Test_GetAll_ReturnsUnauthorized()
    {
        // Arrange
        _mockHelpers.UserManager_ReturnsRandomId(_userManager);

        // Act
        var result = _subject.GetAll();

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}