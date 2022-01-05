using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using nectarineAPI.Controllers;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineData.DataAccess;
using nectarineData.Models;
using Xunit;

namespace nectarineTests.Controllers
{
    public class AuthenticationControllerTests
    {
        private AuthenticationController _subject;
        private Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<ITokenService> _tokenService;
        private readonly NectarineDbContext _mockContext;
        private readonly Mock<ISocialService<GoogleUser>> _mockGoogleService;

        public AuthenticationControllerTests()
        {
            // UserManager setup
            _userManager = MockHelpers.MockUserManager<ApplicationUser>();

            _userManager
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .Returns<string>(email => Task.FromResult(new ApplicationUser { Email = email }));

            // ITokenService setup
            _tokenService = new Mock<ITokenService>();
            
            _tokenService
                .Setup(x => x.GenerateTokenAsync(It.IsAny<ApplicationUser>()))
                .Returns("eySampleJWTString");
            
            // GoogleService setup
            _mockGoogleService = new Mock<ISocialService<GoogleUser>>();
            
            _mockGoogleService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new GoogleUser());
                
            // Database setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _mockContext = new NectarineDbContext(options);
            
            // AuthenticationController setup
            _subject = new AuthenticationController(
                _userManager.Object,
                _tokenService.Object,
                _mockGoogleService.Object,
                _mockContext);
        }

        #region AuthenticateUser

        [Fact(DisplayName = "AuthenticateUser should verify the users details")]
        public async Task Test_AuthenticateUser_ReturnsOK()
        {
            // Arrange
            var createUserDto = new AuthenticateUserDTO
            {
                Email = "test@test.com",
                Password = "Password123",
            };
            
            // Act
            var result = await _subject.AuthenticateUser(createUserDto);
            
            // Arrange
            Assert.IsType<OkObjectResult>(result);
        }
        
        [Fact(DisplayName = "AuthenticateUser should return Unauthorised when an email is not registered")]
        public async Task Test_AuthenticateUser_FailsWhen_EmailIsNotRegistered()
        {
            // Arrange
            _userManager
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()));
            
            var createUserDto = new AuthenticateUserDTO
            {
                Email = "unregisteredEmail@test.com",
                Password = "Password123",
            };
            
            // Act
            var result = await _subject.AuthenticateUser(createUserDto);
            
            // Arrange
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
        
        [Fact(DisplayName = "AuthenticateUser should return Unauthorised when password is wrong")]
        public async Task Test_AuthenticateUser_FailsWhen_PasswordIsIncorrect()
        {
            // Arrange
            var store = new Mock<IUserStore<ApplicationUser>>();
            var passwordHashser = new Mock<IPasswordHasher<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, passwordHashser.Object, null, null, null, null, null, null);

            passwordHashser.Setup(x => x.VerifyHashedPassword(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Failed);

            _userManager = mgr;
            
            _userManager
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .Returns<string>(email => Task.FromResult(new ApplicationUser {Email = email}));
            
            _subject = new AuthenticationController(
                _userManager.Object,
                _tokenService.Object,
                _mockGoogleService.Object,
                _mockContext);
            
            var createUserDto = new AuthenticateUserDTO
            {
                Email = "test@test.com",
                Password = "WrongPassword",
            };
            
            // Act
            var result = await _subject.AuthenticateUser(createUserDto);
            
            // Arrange
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        #endregion

        #region AuthenticateGoogleUser

        [Fact(DisplayName = "AuthenticateGoogleUser should authenticate a Google user and return Ok")]
        public async Task Test_AuthenticateGoogleUser_ReturnsOk()
        {
            // Assert
            var authenticateSocialUserDto = new AuthenticateSocialUserDTO
            {
                Token = "googleToken",
            };
            
            // Act
            var result = await _subject.AuthenticateGoogleUser(authenticateSocialUserDto);
            
            // Arrange
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion
    }
}