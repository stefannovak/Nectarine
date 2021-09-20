using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly Mock<IUserCustomerService> _userCustomerServiceMock;

        public UsersControllerTest()
        {
            // UserManager setup
            Mock<UserManager<ApplicationUser>> _userManager = MockHelpers.MockUserManager<ApplicationUser>();

            _userManager.Setup(manager => manager
                    .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser());

            _userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .Returns<string>((email) => Task.FromResult(new ApplicationUser {Email = email}));

            // UserCustomerService setup
            _userCustomerServiceMock = new Mock<IUserCustomerService>();
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

    }
}