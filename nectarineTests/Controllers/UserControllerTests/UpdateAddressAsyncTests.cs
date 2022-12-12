using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Models.Customers;
using NectarineData.Models;
using Stripe;
using Xunit;

namespace NectarineTests.Controllers.UserControllerTests;

public partial class UsersControllerTest
{
    [Fact(DisplayName = "UpdateAddressAsync should update a users address")]
    public async Task Test_UpdateAddressAsync_ReturnsOk()
    {
        // Arrange
        var updateUserDto = new UpdateAddressDTO
        {
            Address = new AddressOptions
            {
                Line1 = "21 BoolProp Lane",
                City = "Big City",
                Country = "England",
                PostalCode = "11111",
            },
            IsPrimaryAddress = true,
        };

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
            .Returns(new UserCustomerDetails(
                "cus_123",
                "pay_123",
                "test@me.com",
                "me",
                123));

        _userCustomerServiceMock
            .Setup(x => x.UpdateCustomer(
                appUser,
                new CustomerUpdateOptions
                {
                    Address = updateUserDto.Address,
                }))
            .Returns(new Customer());

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
        var result = await _controller.UpdateAddressAsync(It.IsAny<UpdateAddressDTO>());

        // Arrange
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "UpdateAddressAsync should return a Bad Request when a User's Customer can't be fetched.")]
    public async Task Test_UpdateAddressAsync_FailsWhen_CantGetACustomerFromUser()
    {
        // Act
        var result = await _controller.UpdateAddressAsync(It.IsAny<UpdateAddressDTO>());

        // Arrange
        Assert.IsType<BadRequestObjectResult>(result);
    }
}