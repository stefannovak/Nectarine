using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace NectarineTests.Controllers.RatingsControllerTests;

public partial class RatingsControllerTests
{
    [Fact(DisplayName = "DeleteRating should return NoContent")]
    public async Task Test_DeleteRating()
    {
        // Act
        var result = await _subject.DeleteRating(_user.SubmittedRatings.First().Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "DeleteRating should return Unauthorized")]
    public async Task Test_DeleteRating_ReturnsUnauthorized()
    {
        // Arrange
        _mockHelpers.UserManager_ReturnsRandomId(_userManager);

        // Act
        var result = await _subject.DeleteRating(_user.SubmittedRatings.First().Id);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact(DisplayName = "DeleteRating should return NotFound")]
    public async Task Test_DeleteRating_ReturnsNotFound()
    {
        // Act
        var result = await _subject.DeleteRating(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}