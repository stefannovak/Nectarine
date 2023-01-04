using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace NectarineTests.Controllers.AddressControllerTests;

public partial class AddressControllerTests
{
    [Fact(DisplayName = "CreateAddress should return NoContentResult")]
    public async Task Test_CreateAddress_ReturnsOk()
    {
        // Act
        var result = await _subject.CreateAddress(_createAddressDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "CreateAddress should return Unauthorized")]
    public async Task Test_CreateAddress_ReturnsUnauthorized()
    {
        // Arrange
        _mockHelpers.UserManager_ReturnsRandomId(_userManager);

        // Act
        var result = await _subject.CreateAddress(_createAddressDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact(DisplayName = "CreateAddress should return NoContentResult when Is Not Primary Address")]
    public async Task Test_CreateAddress_ReturnsNoContent_WhenIsNotPrimaryAddress()
    {
        // Arrange
        _testDto.IsPrimaryAddress = false;

        // Act
        var result = await _subject.CreateAddress(_createAddressDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}