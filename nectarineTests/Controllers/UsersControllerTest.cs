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
using nectarineData.Models;
using Xunit;

namespace nectarineTests.Controllers
{
    public class UsersControllerTest
    {
        private readonly UsersController _controller;
        private readonly Mock<IMapper> _mockMapper = new ();

        public UsersControllerTest()
        {
            // UserManager setup
            var store = new Mock<IUserPasswordStore<ApplicationUser>>();
            store.Setup(store =>
                    store.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(IdentityResult.Success));
            
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.User.RequireUniqueEmail = true;
            options.Setup(options => options.Value).Returns(idOptions);

            var userValidators = new List<IUserValidator<ApplicationUser>>();
            UserValidator<ApplicationUser> validator = new UserValidator<ApplicationUser>();
            userValidators.Add(validator);

            var passValidator = new PasswordValidator<ApplicationUser>();
            var pwdValidators = new List<IPasswordValidator<ApplicationUser>>();
            pwdValidators.Add(passValidator);
            
            // UserManager setup
            Mock<UserManager<ApplicationUser>> _userManager = new (
                store.Object,
                options.Object,
                new PasswordHasher<ApplicationUser>(),
                userValidators,
                pwdValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
            
            _userManager.Setup(manager => manager
                    .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser());

            _userManager.Setup(manager => manager
                    .CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
            
            // AutoMapper setup
            _mockMapper.Setup(x => x.Map<UserDTO>(It.IsAny<ApplicationUser>()))
                .Returns((ApplicationUser source) => new UserDTO
                {
                    Id = source.Id,
                    Email = source.Email,
                });
            
            _controller = new UsersController(_userManager.Object, _mockMapper.Object);
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