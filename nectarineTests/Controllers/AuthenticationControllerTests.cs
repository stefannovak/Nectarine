using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Controllers;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Models;
using NectarineAPI.Services;
using NectarineAPI.Services.Auth;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using NectarineData.Models.Enums;
using Xunit;

namespace NectarineTests.Controllers
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<ITokenService> _tokenService;
        private readonly NectarineDbContext _mockContext;
        private readonly Mock<IExternalAuthService<GoogleUser>> _mockGoogleService;
        private readonly Mock<IExternalAuthService<MicrosoftUser>> _mockMicrosoftService;
        private readonly Mock<IExternalAuthService<FacebookUser>> _mockFacebookService;
        private readonly Mock<IUserCustomerService> _userCustomerService;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly TelemetryClient _telemetryClient;

        private readonly string googleUserId = Guid.NewGuid().ToString();
        private readonly string microsoftUserId = Guid.NewGuid().ToString();
        private readonly string facebookUserId = Guid.NewGuid().ToString();
        private readonly ApplicationUser _user;
        private readonly AuthenticateSocialUserDTO _authenticateSocialUserDto = new ()
        {
            Token = "googleToken",
        };

        private AuthenticationController _subject;
        private Mock<UserManager<ApplicationUser>> _userManager;

        public AuthenticationControllerTests()
        {
            // User setup
            _user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Nectarine",
                LastName = "User",
                Email = "googleUser@gmail.com",
                SocialLinks = new Collection<ExternalAuthLink>
                {
                    new ()
                    {
                        PlatformId = googleUserId,
                        Platform = ExternalAuthPlatform.Google,
                    },
                },
                PaymentProviderCustomerId = "StripeCustomerId",
            };

            // UserManager setup
            _userManager = MockHelpers.MockUserManager<ApplicationUser>();

            _userManager
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .Returns<string>(email => Task.FromResult(new ApplicationUser { Email = email }));

            _userManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManager
                .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // ITokenService setup
            _tokenService = new Mock<ITokenService>();

            _tokenService
                .Setup(x => x.GenerateTokenAsync(It.IsAny<ApplicationUser>()))
                .Returns("eySampleJWTString");

            // GoogleService setup
            _mockGoogleService = new Mock<IExternalAuthService<GoogleUser>>();

            _mockGoogleService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new GoogleUser(
                    googleUserId,
                    _user.FirstName,
                    _user.LastName,
                    _user.Email));

            // MicrosoftAuthService setup
            _mockMicrosoftService = new Mock<IExternalAuthService<MicrosoftUser>>();

            _mockMicrosoftService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new MicrosoftUser(
                    microsoftUserId,
                    _user.FirstName,
                    _user.LastName,
                    _user.Email));

            // FacebookAuthService setup
            _mockFacebookService = new Mock<IExternalAuthService<FacebookUser>>();

            _mockFacebookService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FacebookUser(
                    facebookUserId,
                    _user.FirstName,
                    _user.LastName,
                    _user.Email));

            // IUserCustomerService setup
            _userCustomerService = new Mock<IUserCustomerService>();

            _userCustomerService
                .Setup(x => x.AddCustomerIdAsync(
                    It.IsAny<ApplicationUser>()))
                .Returns(Task.CompletedTask);

            // IEmailService setup
            _emailServiceMock = new Mock<IEmailService>();

            _emailServiceMock
                .Setup(x => x.SendEmail(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()));

            _emailServiceMock
                .Setup(x => x.SendWelcomeEmail(It.IsAny<string>()));

            // Database setup
            var options = new DbContextOptionsBuilder<NectarineDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _mockContext = new NectarineDbContext(options);

            // Telemetry Client setup
            _telemetryClient = MockHelpers.TestTelemetryClient();

            // AuthenticationController setup
            _subject = new AuthenticationController(
                _userManager.Object,
                _tokenService.Object,
                _mockGoogleService.Object,
                _mockMicrosoftService.Object,
                _mockFacebookService.Object,
                _userCustomerService.Object,
                _emailServiceMock.Object,
                _mockContext,
                _telemetryClient);
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
                .Returns<string>(email => Task.FromResult(new ApplicationUser { Email = email }));

            _subject = new AuthenticationController(
                _userManager.Object,
                _tokenService.Object,
                _mockGoogleService.Object,
                _mockMicrosoftService.Object,
                _mockFacebookService.Object,
                _userCustomerService.Object,
                _emailServiceMock.Object,
                _mockContext,
                _telemetryClient);

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

        #region AuthenticateMicrosoftUser

        [Fact(DisplayName = "AuthenticateMicrosoftUser should authenticate an existing Microsoft user and return Ok")]
        public async Task Test_AuthenticateMicrosoftUser_ReturnsOk()
        {
            // Assert
            await _mockContext.Users.AddAsync(_user);
            await _mockContext.SaveChangesAsync();

            _mockMicrosoftService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new MicrosoftUser(
                    microsoftUserId,
                    _user.FirstName,
                    _user.LastName,
                    _user.Email));

            // Act
            var result = await _subject.AuthenticateMicrosoftUser(_authenticateSocialUserDto);

            // Arrange
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "AuthenticateMicrosoftUser should authenticate a NotFound when a MicrosoftUser can't be found")]
        public async Task Test_AuthenticateMicrosoftUser_FailsWhen_AGoogleUserDoesNotExist()
        {
            // Assert
            _mockMicrosoftService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()));

            // Act
            var result = await _subject.AuthenticateMicrosoftUser(_authenticateSocialUserDto);

            // Arrange
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region AuthenticateFacebookUser

        [Fact(DisplayName = "AuthenticateFacebookUser should authenticate an existing Facebook user and return Ok")]
        public async Task Test_AuthenticateFacebookUser_ReturnsOk()
        {
            // Assert
            await _mockContext.Users.AddAsync(_user);
            await _mockContext.SaveChangesAsync();

            _mockFacebookService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new FacebookUser(
                    facebookUserId,
                    _user.FirstName,
                    _user.LastName,
                    _user.Email));


            // Act
            var result = await _subject.AuthenticateFacebookUser(_authenticateSocialUserDto);

            // Arrange
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "AuthenticateFacebookUser should authenticate a NotFound when a FacebookUser can't be found")]
        public async Task Test_AuthenticateFacebookUser_FailsWhen_AFacebookUserDoesNotExist()
        {
            // Assert
            _mockFacebookService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()));

            // Act
            var result = await _subject.AuthenticateFacebookUser(_authenticateSocialUserDto);

            // Arrange
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region AuthenticateGoogleUser

        [Fact(DisplayName = "AuthenticateGoogleUser should authenticate an existing Google user and return Ok")]
        public async Task Test_AuthenticateGoogleUser_ReturnsOk()
        {
            // Assert
            await _mockContext.Users.AddAsync(_user);
            await _mockContext.SaveChangesAsync();

            _mockGoogleService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(new GoogleUser(
                    googleUserId,
                    _user.FirstName,
                    _user.LastName,
                    _user.Email));

            // Act
            var result = await _subject.AuthenticateGoogleUser(_authenticateSocialUserDto);

            // Arrange
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "AuthenticateGoogleUser should authenticate a NotFound when a GoogleUser can't be found")]
        public async Task Test_AuthenticateGoogleUser_FailsWhen_AGoogleUserDoesNotExist()
        {
            // Assert
            _mockGoogleService
                .Setup(x => x.GetUserFromTokenAsync(It.IsAny<string>()));

            // Act
            var result = await _subject.AuthenticateGoogleUser(_authenticateSocialUserDto);

            // Arrange
            Assert.IsType<NotFoundObjectResult>(result);
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
            var result = await _subject.CreateUserAsync(createUserDto);

            // Arrange
            Assert.IsType<OkObjectResult>(result);
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
            var result = await _subject.CreateUserAsync(createUserDto);

            // Arrange
            Assert.True((result as ObjectResult)?.StatusCode == 500);
        }

        [Fact(DisplayName = "CreateUserAsync should return a Bad Request UserManager is unable to create a user")]
        public async Task Test_CreateUserAsync_FailsWhen_UserManagerFailsToCreateANewUser()
        {
            // Arrange
            _userManager.Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError
                {
                    Code = "555",
                    Description = "A test failure.",
                }));

            var createUserDto = new CreateUserDTO
            {
                Email = "test@test.com",
                Password = "Password123",
            };

            // Act
            var result = await _subject.CreateUserAsync(createUserDto);

            // Arrange
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region CreateExternalAuthUser

        [Fact(DisplayName = "AuthenticateGoogleUser should authenticate and create a Google user and return Ok")]
        public async Task Test_AuthenticateGoogleUser_CreatesGoogleUserAnd_ReturnsOk()
        {
            // The user is set up but not added to the context, so the method does not find a user and so creates one.
            // Act
            var result = await _subject.AuthenticateGoogleUser(_authenticateSocialUserDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "AuthenticateGoogleUser should return a 500 when an exception is throw")]
        public async Task Test_AuthenticateGoogleUser_ThrowsAnd_Returns500()
        {
            // Arrange
            _userManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
                .Throws(new Exception("Error", new Exception("Exception")));

            // Act
            var result = await _subject.AuthenticateGoogleUser(_authenticateSocialUserDto);

            // Assert
            Assert.True((result as ObjectResult)?.StatusCode == 500);
        }

        [Fact(DisplayName = "AuthenticateGoogleUser should return a BadRequest when the user can't be created")]
        public async Task Test_AuthenticateGoogleUser_FailsWhen_AUserCanNotBeCreated()
        {
            // Arrange
            _userManager
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(errors: new[] { new IdentityError { Code = "Terrible", Description = "Awful" } }));

            // Act
            var result = await _subject.AuthenticateGoogleUser(_authenticateSocialUserDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion
    }
}