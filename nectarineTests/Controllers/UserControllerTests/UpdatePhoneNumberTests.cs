using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineAPI.DTOs.Requests;
using NectarineData.Models;
using Xunit;

namespace NectarineTests.Controllers.UserControllerTests;

public partial class UsersControllerTest
{
    [Fact(DisplayName = "UpdatePhoneNumber should return NoContent")]
    public async Task Test_UpdatePhoneNumber()
    {
        // Act
        var result = await _subject.UpdatePhoneNumber(_updatePhoneNumberDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "UpdatePhoneNumber should return Unauthorized")]
    public async Task Test_UpdatePhoneNumber_ReturnsUnauthorized()
    {
        // Arrange
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _subject.UpdatePhoneNumber(_updatePhoneNumberDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact(DisplayName = "UpdatePhoneNumber should return BadRequest")]
    public async Task Test_UpdatePhoneNumber_ReturnsBadRequest()
    {
        // Arrange
        _userManager
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _subject.UpdatePhoneNumber(_updatePhoneNumberDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}