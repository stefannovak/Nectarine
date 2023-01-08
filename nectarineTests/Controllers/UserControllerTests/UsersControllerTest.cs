using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
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
using Xunit;

namespace NectarineTests.Controllers.UserControllerTests;

public partial class UsersControllerTest
{
    private readonly UsersController _controller;
    private readonly Mock<IMapper> _mockMapper = new ();
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly NectarineDbContext _mockContext;
    private readonly Mock<IUserCustomerService> _userCustomerServiceMock;

    private readonly UserCustomerDetails _userCustomerDetails = new (
        "cus_123",
        "pay_123",
        "test@me.com",
        "123123123123",
        "me",
        123,
        new UserAddress(
            "21 BoolProp Lane",
            null,
            "Big City",
            "11111",
            "UK",
            true));

    private readonly ApplicationUser _user = new ()
    {
        Id = Guid.NewGuid().ToString(),
        PhoneNumber = "123123123123",
        VerificationCode = 123123,
        PhoneNumberConfirmed = false,
    };

    private Confirm2FACodeDTO confirm2FACodeDTO = new () { Code = 123123 };

    public UsersControllerTest()
    {
        // UserManager setup
        _userManager = MockHelpers.MockUserManager<ApplicationUser>();

        _userManager
            .Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_user);

        _userManager
            .Setup(manager => manager
                .GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(_user.Id);

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
            .Setup(x => x.GetCustomer(It.IsAny<string>()))
            .Returns(_userCustomerDetails);

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

        // IBackgroundJobs setup
        var backgroundJobsMock = new Mock<IBackgroundJobClient>();

        // Database setup
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _mockContext = new NectarineDbContext(options);
        _mockContext.Users.Add(_user);
        _mockContext.SaveChangesAsync();

        _controller = new UsersController(
            _userManager.Object,
            _mockMapper.Object,
            _userCustomerServiceMock.Object,
            _mockContext,
            phoneServiceMock.Object,
            backgroundJobsMock.Object);
    }

    #region GetCurrentAsync

    [Fact(DisplayName = "GetCurrentAsync should get the current user and return an Ok")]
    public void Test_GetCurrentAsyncTest()
    {
        // Act
        var result = _controller.GetCurrentAsync();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetCurrentAsync should fail to get the current user and return a BadRequest")]
    public void Test_GetCurrentAsyncTest_ReturnsBadRequestWhen_FailsToGetAUser()
    {
        // Arrange
        _userManager.Setup(manager => manager
            .GetUserId(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = _controller.GetCurrentAsync();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region DeleteUserAsync

    [Fact(DisplayName = "DeleteAsync should delete the current user and return NoContent")]
    public async Task Test_DeleteAsync_ReturnsNoContent()
    {
        // Act
        var result = await _controller.DeleteAsync();

        // Arrange
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "DeleteAsync should delete the current user from the context")]
    public async Task Test_DeleteAsync_DeletesUser()
    {
        // Act
        await _controller.DeleteAsync();
        var deletedUser = _mockContext.Users.FirstOrDefault(x => x.Id == _user.Id);

        // Arrange
        Assert.Null(deletedUser);
    }

    [Fact(DisplayName = "DeleteAsync should return unauthorized when a user can't be found")]
    public async Task Test_DeleteAsync_ReturnsUnauthorized()
    {
        // Arrange
        _userManager.Setup(manager => manager
            .GetUserId(It.IsAny<ClaimsPrincipal>()));

        // Act
        var result = await _controller.DeleteAsync();

        // Arrange
        Assert.IsType<UnauthorizedResult>(result);
    }

    #endregion

    #region SendVerificationCode

    [Fact(DisplayName = "SendVerificationCode should return NoContentResult.")]
    public async Task Test_SendVerificationCode_ReturnsNoContentResult()
    {
        // Act
        var result = await _controller.GetVerificationCode();

        // Assert
        Assert.IsType<NoContentResult>(result);
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

    [Fact(DisplayName = "Confirm2FACode should return NoContentResult")]
    public async Task Test_Confirm2FACode_ReturnsNoContentResult()
    {
        // Act
        var result = await _controller.Confirm2FACode(confirm2FACodeDTO);

        // Assert
        Assert.IsType<NoContentResult>(result);
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

    [Fact(DisplayName = "Confirm2FACode should return BadRequest when the code is null")]
    public async Task Test_Confirm2FACode_ReturnsBadRequestWhen_CodeOrExpiryIsNull()
    {
        // Assert
        _userManager
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ApplicationUser
            {
                VerificationCode = null,
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