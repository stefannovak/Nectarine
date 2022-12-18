using System;
using System.Threading.Tasks;
using NectarineData.Models;
using Xunit;

namespace NectarineTests.Controllers.UserControllerTests;

public partial class UsersControllerTest
{
    [Fact(DisplayName = "DeleteVerificationCodeForUser should complete normally")]
    public async Task Test_DeleteVerificaitonCodeForUser_Completes()
    {
        // Arrange
        _mockContext.Users.Add(_user);
        await _mockContext.SaveChangesAsync();

        // Act
        await _controller.DeleteVerificationCodeForUser(_user.Id);

        // Assert
        Assert.Null(_user.VerificationCode);
    }

    [Fact(DisplayName = "DeleteVerificationCodeForUser should return when no user for found for Id")]
    public async Task Test_DeleteVerificaitonCodeForUser_ReturnsWhen_UserNotFound()
    {
        // Act
        await _controller.DeleteVerificationCodeForUser(_user.Id);

        // Assert
        Assert.NotNull(_user.VerificationCode);
    }
}