using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using nectarineAPI.Controllers;
using nectarineAPI.DTOs.Generic;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.Services;
using nectarineData.Models;
using Stripe;
using Xunit;

namespace nectarineTests.Controllers
{
    public class UsersControllerTest
    {
        private readonly UsersController _controller;
        private readonly Mock<IMapper> _mockMapper = new ();
        private readonly Mock<UserManager<ApplicationUser>> _userManager;

        public UsersControllerTest()
        {
            // UserManager setup
            _userManager = MockHelpers.MockUserManager<ApplicationUser>();

            _userManager.Setup(manager => manager
                    .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser());

            _userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .Returns<string>((email) => Task.FromResult(new ApplicationUser {Email = email}));

            // UserCustomerService setup
            Mock<IUserCustomerService> _userCustomerServiceMock = new ();
            _userCustomerServiceMock
                .Setup(x => x.AddStripeCustomerIdAsync(It.IsAny<ApplicationUser>(), new CustomerCreateOptions()))
                .Returns(Task.CompletedTask);
            
            // AutoMapper setup
            _mockMapper.Setup(x => x.Map<UserDTO>(It.IsAny<ApplicationUser>()))
                .Returns((ApplicationUser source) => new UserDTO
                {
                    Id = source.Id,
                    Email = source.Email,
                });
            
            _controller = new UsersController(_userManager.Object, _mockMapper.Object, _userCustomerServiceMock.Object);
        }
        
        [Fact(DisplayName = "GetCurrentAsync should get the current user and return an Ok")]
        public async Task Test_GetCurrentAsyncTest()
        {
            // Act
            var result = await _controller.GetCurrentAsync();
            
            // Arrange
            Assert.IsType<OkObjectResult>(result);
        }
        
        # region CreateUserAsync 
        
        [Fact(DisplayName = "CreateUserAsync should create a user")]
        public async Task Test_CreateUserAsync()
        {
            // Arrange
            var createUserDto = new CreateUserDTO
            {
                Email = "test@test.com",
                Password = "Password123",
            };
            
            // Act
            var result = await _controller.CreateUserAsync(createUserDto);
            
            // Arrange
            Assert.IsType<CreatedResult>(result);
        }
        
        [Fact(DisplayName = "CreateUserAsync should return a BadRequest when given an empty email")]
        public async Task Test_CreateUserAsync_FailsWhenEmailIsEmpty()
        {
            // Arrange
            var createUserDto = new CreateUserDTO
            {
                Email = "",
                Password = "Password123",
            };
            
            // Act
            var result = await _controller.CreateUserAsync(createUserDto);
            
            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }
        
        [Fact(DisplayName = "CreateUserAsync should return an Internal Server Error when UserManager goes wrong")]
        public async Task Test_CreateUserAsync_FailsWhenUserManagerFailsToFetchTheNewUser()
        {
            // Arrange
            _userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()));
            
            var createUserDto = new CreateUserDTO
            {
                Email = "test@test.com",
                Password = "Password123",
            };
            
            // Act
            var result = await _controller.CreateUserAsync(createUserDto);
            
            // Arrange
            Assert.IsType<ObjectResult>(result);
            Assert.True((result as ObjectResult)?.StatusCode == 500);
        }
        
        [Fact(DisplayName = "CreateUserAsync should return a Bad Request UserManager is unable to create a user")]
        public async Task Test_CreateUserAsync_FailsWhenUserManagerFailsToCreateANewUser()
        {
            // Arrange
            _userManager.Setup(x  => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "555",
                    Description = "A test failure."
                }));
                     
            var createUserDto = new CreateUserDTO
            {
                Email = "test@test.com",
                Password = "Password123",
            };
                     
            // Act
            var result = await _controller.CreateUserAsync(createUserDto);
                     
            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }
        
        # endregion
    }
}