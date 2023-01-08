using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace NectarineTests.Controllers.RatingsControllerTests;

public partial class RatingsControllerTests
{
    [Fact(DisplayName = "CreateRating should return NoContent")]
    public async Task Test_CreateRating()
    {
        // Act
        var result = await _subject.CreateRating(_testRatingDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "CreateRating should return Unauthorized")]
    public async Task Test_CreateRating_ReturnsUnauthorized()
    {
        // Arrange
        _mockHelpers.UserManager_GetUserAsync_ReturnsNothing(_userManager);

        // Act
        var result = await _subject.CreateRating(_testRatingDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}