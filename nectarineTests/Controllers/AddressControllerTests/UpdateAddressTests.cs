using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.DTOs.Generic;
using NectarineData.Models;
using Xunit;

namespace NectarineTests.Controllers.AddressControllerTests;

public partial class AddressControllerTests
{
    [Fact(DisplayName = "UpdateAddress should return NoContent")]
    public async Task Test_UpdateAddress()
    {
        // Act
        var result = await _subject.UpdateAddressAsync(new UserAddressDTO(
                _user.UserAddresses.First().Id,
                "123",
                "Road",
                "London",
                "12312",
                "UK"));

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "UpdateAddress should return Unauthorized")]
    public async Task Test_UpdateAddress_ReturnsUnauthorized()
    {
        // Arrange
        _mockHelpers.UserManager_ReturnsRandomId(_userManager);

        // Act
        var result = await _subject.UpdateAddressAsync(It.IsAny<UserAddressDTO>());

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact(DisplayName = "UpdateAddress should return BadRequest")]
    public async Task Test_UpdateAddress_ReturnsBadRequest()
    {
        // Arrange
        var address = new UserAddressDTO(
            Guid.NewGuid(),
            "111",
            "Road",
            "London",
            "12312",
            "UK");

        // Act
        var result = await _subject.UpdateAddressAsync(address);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "UpdateAddress should return NoContentResult with a request setting it to a Primary address")]
    public async Task Test_UpdateAddress_WhenPrimaryAddress_ReturnsNoContentResult()
    {
        // Arrange
        _user.UserAddresses = new List<UserAddress>
        {
            new ("555",
                "Lane",
                "Manc",
                "11111",
                "UK"),
            new (
                "123",
                "Lane",
                "Manc",
                "55555",
                "UK",
                true),
        };

        _user.Id = Guid.NewGuid().ToString();
        _mockContext.Users.Add(_user);
        await _mockContext.SaveChangesAsync();

        _userManager
            .Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(_user.Id);

        // Act
        var result = await _subject.UpdateAddressAsync(new UserAddressDTO(
                _user.UserAddresses.First().Id,
                "123",
                "Road",
                "London",
                "11111",
                "UK",
                true));

        // Assert
        Assert.False(_user.UserAddresses.FirstOrDefault(x => x.Postcode == "55555")?.IsPrimaryAddress);
        Assert.IsType<NoContentResult>(result);
    }
}