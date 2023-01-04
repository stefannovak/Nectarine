using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace NectarineTests.Controllers.AddressControllerTests;

public partial class AddressControllerTests
{
    [Fact(DisplayName = "DeleteById should return NoContentResult")]
    public async Task Test_DeleteById()
    {
        // Act
        var result = await _subject.DeleteById(_user.UserAddresses.First().Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "DeleteById should return Unauthorized")]
    public async Task Test_DeleteById_ReturnsUnauthorized()
    {
        // Arrange
        _mockHelpers.UserManager_ReturnsRandomId(_userManager);

        // Act
        var result = await _subject.DeleteById(_user.UserAddresses.First().Id);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact(DisplayName = "DeleteById should return NotFound")]
    public async Task Test_DeleteById_ReturnsNotFound()
    {
        // Act
        var result = await _subject.DeleteById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}