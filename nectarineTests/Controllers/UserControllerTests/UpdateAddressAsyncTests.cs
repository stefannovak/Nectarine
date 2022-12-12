using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineAPI.Models.Customers;
using NectarineData.Models;
using Xunit;

namespace NectarineTests.Controllers.UserControllerTests;

public partial class UsersControllerTest
{
    [Fact(DisplayName = "UpdateAddressAsync should update a users address")]
    public async Task Test_UpdateAddressAsync_ReturnsOk()
    {
        // Arrange
        var updateUserDto = new UserAddress
        (
            "21 BoolProp Lane",
            null,
            "Big City",
           "11111",
            "UK",
            true
        );

        var appUser = new ApplicationUser
        {
            Email = "test@test.com",
        };

        _userCustomerServiceMock
            .Setup(x => x.AddCustomerIdAsync(
                It.IsAny<ApplicationUser>()))
            .Returns(Task.CompletedTask);

        _userManager.Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(appUser);

        _userCustomerServiceMock
            .Setup(x => x.GetCustomer(appUser.PaymentProviderCustomerId))
            .Returns(_userCustomerDetails);

        _userCustomerServiceMock
            .Setup(x => x.UpdateCustomerAddress(appUser.PaymentProviderCustomerId, new UserAddress
            (
                "21 BoolProp Lane",
                null,
                "Big City",
                "11111",
                "UK",
                true
            )))
            .Returns(_userCustomerDetails);

        // Act
        var result = await _controller.UpdateAddressAsync(updateUserDto);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact(DisplayName = "UpdateAddressAsync should return a Bad Request when a User can't be fetched.")]
    public async Task Test_UpdateAddressAsync_FailsWhen_CantGetAUser()
    {
        // Arrange
        _userManager.Setup(manager => manager
            .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _controller.UpdateAddressAsync(It.IsAny<UserAddress>());

        // Arrange
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "UpdateAddressAsync should return a Bad Request when a User's Customer can't be fetched.")]
    public async Task Test_UpdateAddressAsync_FailsWhen_CantGetACustomerFromUser()
    {
        // Arrange
        _userCustomerServiceMock
            .Setup(x => x.GetCustomer(It.IsAny<string>()));

        // Act
        var result = await _controller.UpdateAddressAsync(It.IsAny<UserAddress>());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}