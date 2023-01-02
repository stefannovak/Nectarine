using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace NectarineTests.Controllers.AddressControllerTests;

public partial class AddressControllerTests
{
    [Fact(DisplayName = "CreateAddress should return Ok")]
    public async Task Test_CreateAddress_ReturnsOk()
    {
        // Act
        var result = await _subject.CreateAddress(_testDto);
        
        // Assert
        Assert.IsType<OkResult>(result);
    }
    
    [Fact(DisplayName = "CreateAddress should return Unauthorized")]
    public async Task Test_CreateAddress_ReturnsUnauthorized()
    {
        // Arrange
        _mockHelpers.UserManager_ReturnsRandomId(_userManager);
        
        // Act
        var result = await _subject.CreateAddress(_testDto);
        
        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
    
    [Fact(DisplayName = "CreateAddress should return WhenIsNotPrimaryAddress")]
    public async Task Test_CreateAddress_ReturnsOk_WhenIsNotPrimaryAddress()
    {
        // Arrange
        var testDto = _testDto;
        testDto.IsPrimaryAddress = false;
        
        // Act
        var result = await _subject.CreateAddress(testDto);
        
        // Assert
        Assert.IsType<OkResult>(result);
    }
}