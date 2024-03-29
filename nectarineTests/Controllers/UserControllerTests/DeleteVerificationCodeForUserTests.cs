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
        // Act
        await _subject.DeleteVerificationCodeForUser(_user.Id.ToString());

        // Assert
        Assert.Null(_user.VerificationCode);
    }

    [Fact(DisplayName = "DeleteVerificationCodeForUser should return when no user for found for Id")]
    public async Task Test_DeleteVerificaitonCodeForUser_ReturnsWhen_UserNotFound()
    {
        // Act
        await _subject.DeleteVerificationCodeForUser(Guid.NewGuid().ToString());

        // Assert
        Assert.NotNull(_user.VerificationCode);
    }
}