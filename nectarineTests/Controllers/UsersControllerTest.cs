using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using nectarineAPI.Controllers;
using nectarineAPI.DTOs.Generic;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.Services;
using nectarineData.DataAccess;
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
        private readonly NectarineDbContext _mockContext;
        private readonly Mock<IUserCustomerService> _userCustomerServiceMock;

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
            _userCustomerServiceMock = new Mock<IUserCustomerService>();
            
            _userCustomerServiceMock
                .Setup(x => x.AddStripeCustomerIdAsync(
                    It.IsAny<ApplicationUser>(),
                    new CustomerCreateOptions()))
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
                .Setup(x => x.GetCustomer(It.IsAny<ApplicationUser>()))
                .Returns(It.IsAny<Customer>());
            
            // AutoMapper setup
            _mockMapper.Setup(x => x.Map<UserDTO>(It.IsAny<ApplicationUser>()))
                .Returns((ApplicationUser source) => new UserDTO
                {
                    Id = source.Id,
                    Email = source.Email,
                });
            
            // Database setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _mockContext = new NectarineDbContext(options);
            
            _controller = new UsersController(
                _userManager.Object,
                _mockMapper.Object,
                _userCustomerServiceMock.Object,
                _mockContext);
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

        #region CreateUserAsync 
        
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
        public async Task Test_CreateUserAsync_FailsWhen_EmailIsEmpty()
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
        public async Task Test_CreateUserAsync_FailsWhen_UserManagerFailsToFetchTheNewUser()
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
        public async Task Test_CreateUserAsync_FailsWhen_UserManagerFailsToCreateANewUser()
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
        
        #region DeleteUserAsync

        [Fact(DisplayName = "DeleteAsync should delete the current user and return an Ok")]
        public async Task Test_DeleteAsync()
        {
            // Arrange
            var appUser = new ApplicationUser
            {
                Email = "test@test.com"
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
                Email = "test@test.com"
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

        #region UpdateUserAsync

        [Fact(DisplayName = "UpdateUserAsync should update the current user and return an OK")]
        public async Task Test_UpdateUserAsync_ReturnsOk()
        {
            // Arrange
            var updateUserDto = new UpdateUserDTO
            {
                Email = "newEmail@test.com",
                Address = new AddressOptions
                {
                    Line1 = "21 BoolProp Lane",
                    City = "Big City",
                    Country = "England",
                    PostalCode = "11111",
                },
            };
            
            var appUser = new ApplicationUser
            {
                Email = "test@test.com",
            };
            
            _userCustomerServiceMock
                .Setup(x => x.AddStripeCustomerIdAsync(
                    appUser,
                    new CustomerCreateOptions()))
                .Returns(Task.CompletedTask);
            
            _userManager.Setup(manager => manager
                    .GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(appUser);
            
            _userCustomerServiceMock
                .Setup(x => x.GetCustomer(appUser))
                .Returns(new Customer());
            
            _userCustomerServiceMock
                .Setup(x => x.UpdateCustomer(
                    appUser,
                    new CustomerUpdateOptions
                    {
                        Address = updateUserDto.Address,
                        Email = updateUserDto.Email,
                    }))
                .Returns(new Customer {Email = updateUserDto.Email});

            // Act
            var result = await _controller.UpdateUserAsync(updateUserDto);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "UpdateUserAsync should return a Bad Request when a User can't be fetched.")]
        public async Task Test_UpdateUserAsync_FailsWhen_CantGetAUser()
        {
            // Arrange
            _userManager.Setup(manager => manager
                .GetUserAsync(It.IsAny<ClaimsPrincipal>()));
            
            // Act
            var result = await _controller.UpdateUserAsync(It.IsAny<UpdateUserDTO>());
            
            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }
        
        [Fact(DisplayName = "UpdateUserAsync should return a Bad Request when a User's Customer can't be fetched.")]
        public async Task Test_UpdateUserAsync_FailsWhen_CantGetACustomerFromUser()
        {
            // Arrange
            
            // Act
            var result = await _controller.UpdateUserAsync(It.IsAny<UpdateUserDTO>());
            
            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion
    }
}