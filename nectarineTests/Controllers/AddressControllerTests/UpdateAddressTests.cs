using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.DTOs.Requests;
using NectarineData.Models;
using Xunit;

namespace NectarineTests.Controllers.AddressControllerTests;

public partial class AddressControllerTests
{
    [Fact(DisplayName = "UpdateAddress should return Ok")]
    public async Task Test_UpdateAddress()
    {
        // Arrange
        var previousAddress = new UserAddress(
            "111",
            "Road",
            "London",
            "12312",
            "UK");

        var newAddress = new UserAddressDTO(
            "123",
            "Road",
            "London",
            "12312",
            "UK");

        _user.UserAddresses.Add(previousAddress);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _subject.UpdateAddressAsync(new UpdateAddressDTO(
            newAddress,
            previousAddress.Id));

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact(DisplayName = "UpdateAddress should return Unauthorized")]
    public async Task Test_UpdateAddress_ReturnsUnauthorized()
    {
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _subject.UpdateAddressAsync(It.IsAny<UpdateAddressDTO>());

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact(DisplayName = "UpdateAddress should return BadRequest")]
    public async Task Test_UpdateAddress_ReturnsBadRequest()
    {
        // Arrange
        var address = new UserAddressDTO(
            "111",
            "Road",
            "London",
            "12312",
            "UK");

        // Act
        var result = await _subject.UpdateAddressAsync(new UpdateAddressDTO(
            address,
            Guid.NewGuid()));

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}