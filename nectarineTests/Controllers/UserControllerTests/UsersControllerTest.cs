using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Controllers;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Models.Customers;
using NectarineAPI.Services;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using Stripe;
using Xunit;

namespace NectarineTests.Controllers.UserControllerTests;

public partial class UsersControllerTest
{
    private readonly UsersController _controller;
    private readonly Mock<IMapper> _mockMapper = new ();
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly NectarineDbContext _mockContext;
    private readonly Mock<IUserCustomerService> _userCustomerServiceMock;
    private readonly UpdatePhoneNumberDTO updatePhoneNumberDTO = new ()
    {
        PhoneNumber = "123123123123",
    };

    private Confirm2FACodeDTO confirm2FACodeDTO = new () { Code = 123123 };

    public UsersControllerTest()
    {
        // UserManager setup
        _userManager = MockHelpers.MockUserManager<ApplicationUser>();

        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser
            {
                PhoneNumber = "123123123123",
                VerificationCode = 123123,
                VerificationCodeExpiry = DateTime.Now.AddMinutes(2),
                PhoneNumberConfirmed = false,
            });

        _userManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .Returns<string>((email) => Task.FromResult(new ApplicationUser { Email = email }));

        _userManager
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // UserCustomerService setup
        _userCustomerServiceMock = new Mock<IUserCustomerService>();

        _userCustomerServiceMock
            .Setup(x => x.AddCustomerIdAsync(
                It.IsAny<ApplicationUser>()))
            .Returns(Task.CompletedTask);

        _userCustomerServiceMock
            .Setup(x => x.DeleteCustomer(It.IsAny<ApplicationUser>()))
            .Returns(true);

        _userCustomerServiceMock
            .Setup(x => x.UpdateCustomer(
                It.IsAny<ApplicationUser>(),
                It.IsAny<CustomerUpdateOptions>()))
            .Returns(It.IsAny<Customer>());

        _userCustomerServiceMock
            .Setup(x => x.GetCustomer(It.IsAny<string>()))
            .Returns(new UserCustomerDetails(
                "cus_123",
                "pay_123",
                "test@me.com",
                "me",
                123));

        // IPhoneService setup
        var phoneServiceMock = new Mock<IPhoneService>();

        phoneServiceMock
            .Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()));

        // AutoMapper setup
        _mockMapper.Setup(x => x.Map<UserDTO>(It.IsAny<ApplicationUser>()))
            .Returns((ApplicationUser source) => new UserDTO
            {
                Id = source.Id,
                Email = source.Email,
            });

        // Database setup
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _mockContext = new NectarineDbContext(options);

        _controller = new UsersController(
            _userManager.Object,
            _mockMapper.Object,
            _userCustomerServiceMock.Object,
            _mockContext,
            phoneServiceMock.Object);
    }

    #region GetCurrentAsync

    [Fact(DisplayName = "GetCurrentAsync should get the current user and return an Ok")]
    public async Task Test_GetCurrentAsyncTest()
    {
        // Act
        var result = await _controller.GetCurrentAsync();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetCurrentAsync should fail to get the current user and return a BadRequest")]
    public async Task Test_GetCurrentAsyncTest_ReturnsBadRequestWhen_FailsToGetAUser()
    {
        // Arrange
        _userManager.Setup(manager => manager
            .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _controller.GetCurrentAsync();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region DeleteUserAsync

    [Fact(DisplayName = "DeleteAsync should delete the current user and return an Ok")]
    public async Task Test_DeleteAsync()
    {
        // Arrange
        var appUser = new ApplicationUser
        {
            Email = "test@test.com",
        };

        await _userManager.Object.CreateAsync(appUser);

        var user = await _userManager.Object.FindByEmailAsync(appUser.Email);

        _mockContext.Users.Add(user);
        await _mockContext.SaveChangesAsync();

        _userManager.Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.DeleteAsync();
        var deletedUser = _mockContext.Users.FirstOrDefault(x => x.Id == user.Id);

        // Arrange
        Assert.IsType<OkResult>(result);
        Assert.Null(deletedUser);
    }

    [Fact(DisplayName = "DeleteAsync should return a Bad Request when UserManager fails to get the user")]
    public async Task Test_DeleteAsync_ReturnsBadRequestWhen_CantFetchAUser()
    {
        // Arrange
        _userManager.Setup(manager => manager
            .GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _controller.DeleteAsync();

        // Arrange
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "DeleteAsync should return a Bad Request when UserManager fails to get the user from the database")]
    public async Task Test_DeleteAsync_ReturnsBadRequestWhen_CantFetchAUserFromTheDatabase()
    {
        // Act
        var result = await _controller.DeleteAsync();

        // Arrange
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "DeleteAsync should return a Bad Request when Stripe is unable to delete a customer")]
    public async Task Test_DeleteAsync_ReturnsBadRequestWhen_UserHasNoCustomerObject()
    {
        // Arrange
        var appUser = new ApplicationUser
        {
            Email = "test@test.com",
        };

        await _userManager.Object.CreateAsync(appUser);
        var user = await _userManager.Object.FindByEmailAsync(appUser.Email);
        _mockContext.Users.Add(user);
        await _mockContext.SaveChangesAsync();

        _userManager.Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _userCustomerServiceMock
            .Setup(x => x.DeleteCustomer(It.IsAny<ApplicationUser>()))
            .Returns(false);

        // Act
        var result = await _controller.DeleteAsync();

        // Arrange
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region UpdatePhoneNumber

    [Fact(DisplayName = "UpdatePhoneNumber should take a phone number and attach it to the user.")]
    public async Task Test_UpdatePhoneNumber_ReturnsOk()
    {
        // Act
        var result = await _controller.UpdatePhoneNumber(updatePhoneNumberDTO);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact(DisplayName = "UpdatePhoneNumber should return Unauthorized when a User can't be found.")]
    public async Task Test_UpdatePhoneNumber_ReturnsUnauthorized()
    {
        // Assert
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _controller.UpdatePhoneNumber(updatePhoneNumberDTO);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact(DisplayName = "UpdatePhoneNumber should return BadRequest when the User fails to update.")]
    public async Task Test_UpdatePhoneNumber_ReturnsBadRequest()
    {
        // Assert
        _userManager
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _controller.UpdatePhoneNumber(updatePhoneNumberDTO);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region SendVerificationCode

    [Fact(DisplayName = "SendVerificationCode should return OK.")]
    public async Task Test_SendVerificationCode_ReturnsOk()
    {
        // Act
        var result = await _controller.GetVerificationCode();

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact(DisplayName = "SendVerificationCode should return Unauthorized when a user can't be found.")]
    public async Task Test_SendVerificationCode_ReturnsUnauthorized()
    {
        // Assert
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _controller.GetVerificationCode();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact(DisplayName = "SendVerificationCode should return BadRequest when a user does not have a phone number.")]
    public async Task Test_SendVerificationCode_ReturnsBadRequest()
    {
        // Assert
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser());

        // Act
        var result = await _controller.GetVerificationCode();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region Confirm2FACode

    [Fact(DisplayName = "Confirm2FACode should return Ok")]
    public async Task Test_Confirm2FACode_ReturnsOk()
    {
        // Act
        var result = await _controller.Confirm2FACode(confirm2FACodeDTO);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact(DisplayName = "Confirm2FACode should return Unauthorized")]
    public async Task Test_Confirm2FACode_ReturnsUnauthorized()
    {
        // Assert
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _controller.Confirm2FACode(confirm2FACodeDTO);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact(DisplayName = "Confirm2FACode should return BadRequest when the code or expiry is null")]
    public async Task Test_Confirm2FACode_ReturnsBadRequestWhen_CodeOrExpiryIsNull()
    {
        // Assert
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser
            {
                VerificationCode = null,
                VerificationCodeExpiry = null,
            });

        // Act
        var result = await _controller.Confirm2FACode(confirm2FACodeDTO);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "Confirm2FACode should return BadRequest when the code has expired")]
    public async Task Test_Confirm2FACode_ReturnsBadRequestWhen_CodeExpired()
    {
        // Assert
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser
            {
                VerificationCode = 123123,
                VerificationCodeExpiry = DateTime.Now.Subtract(TimeSpan.FromHours(1)),
            });

        // Act
        var result = await _controller.Confirm2FACode(confirm2FACodeDTO);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "Confirm2FACode should return BadRequest when the code is invalid and has not expired")]
    public async Task Test_Confirm2FACode_ReturnsBadRequestWhen_CodeIsInvalid()
    {
        // Assert
        confirm2FACodeDTO = new Confirm2FACodeDTO
        {
            Code = 111111,
        };

        // Act
        var result = await _controller.Confirm2FACode(confirm2FACodeDTO);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion
}