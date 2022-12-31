using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.DTOs.Responses;
using NectarineAPI.Models;
using NectarineAPI.Services;
using NectarineAPI.Services.Auth;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using NectarineData.Models.Enums;
using Serilog;

namespace NectarineAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IExternalAuthService<GoogleUser> _googleService;
        private readonly IExternalAuthService<MicrosoftUser> _microsoftService;
        private readonly IExternalAuthService<FacebookUser> _facebookService;
        private readonly IUserCustomerService _userCustomerService;
        private readonly IEmailService _emailService;
        private readonly NectarineDbContext _context;
        private readonly TelemetryClient _telemetryClient;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IExternalAuthService<GoogleUser> googleService,
            IExternalAuthService<MicrosoftUser> microsoftService,
            IExternalAuthService<FacebookUser> facebookService,
            IUserCustomerService userCustomerService,
            IEmailService emailService,
            NectarineDbContext context,
            TelemetryClient telemetryClient)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _googleService = googleService;
            _microsoftService = microsoftService;
            _facebookService = facebookService;
            _userCustomerService = userCustomerService;
            _emailService = emailService;
            _context = context;
            _telemetryClient = telemetryClient;
        }

        [HttpPost]
        [Route("Authenticate")]
        public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticateUserDTO authenticateUserDto)
        {
            var user = await _userManager.FindByEmailAsync(authenticateUserDto.Email);
            if (user is null)
            {
                return Unauthorized(new ApiError("Authentication failed. User not found."));
            }

            var passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                authenticateUserDto.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new ApiError("Authentication failed. Incorrect password."));
            }

            return Ok(new
            {
                Token = _tokenService.GenerateTokenAsync(user),
            });
        }

        /// <summary>
        /// Authenticate a user after they have signed in with Microsoft.
        /// </summary>
        /// <param name="authenticateSocialUserDto"></param>
        /// <returns>The user access token.</returns>
        [HttpPost]
        [Route("Microsoft")]
        public async Task<IActionResult> AuthenticateMicrosoftUser([FromBody] AuthenticateSocialUserDTO authenticateSocialUserDto)
        {
            var microsoftUser = await _microsoftService.GetUserFromTokenAsync(authenticateSocialUserDto.Token);
            if (microsoftUser?.Id is null ||
                microsoftUser.FirstName is null ||
                microsoftUser.LastName is null)
            {
                return NotFound(new ApiError(
                $"Could not find a Microsoft user from the given token. Token: {authenticateSocialUserDto.Token}"));
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == microsoftUser.Email &&
                                                          u.SocialLinks
                                                              .Any(x => x.PlatformId == microsoftUser.Id &&
                                                                        x.Platform == ExternalAuthPlatform.Microsoft));

            return user == null
                ? await CreateExternalAuthUser(
                    microsoftUser.Id,
                    microsoftUser.Email,
                    microsoftUser.FirstName,
                    microsoftUser.LastName,
                    ExternalAuthPlatform.Microsoft)
                : Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }

        /// <summary>
        /// Authenticate a user after they have signed in with Facebook.
        /// Playground at https://developers.facebook.com/tools/explorer/.
        /// </summary>
        /// <param name="authenticateSocialUserDto"></param>
        /// <returns>The user access token.</returns>
        [HttpPost]
        [Route("Facebook")]
        public async Task<IActionResult> AuthenticateFacebookUser([FromBody] AuthenticateSocialUserDTO authenticateSocialUserDto)
        {
            var facebookUser = await _facebookService.GetUserFromTokenAsync(authenticateSocialUserDto.Token);
            if (facebookUser?.Id is null ||
                facebookUser.FirstName is null ||
                facebookUser.LastName is null)
            {
                return NotFound(new ApiError(
                    $"Could not find a Facebook user from the given token. Token: {authenticateSocialUserDto.Token}"));
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == facebookUser.Email &&
                                                          u.SocialLinks
                                                              .Any(x => x.PlatformId == facebookUser.Id &&
                                                                        x.Platform == ExternalAuthPlatform.Facebook));

            return user == null
                ? await CreateExternalAuthUser(
                    facebookUser,
                    facebookUser.FirstName,
                    facebookUser.LastName,
                    ExternalAuthPlatform.Facebook)
                : Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }

        /// <summary>
        /// Authenticate a user after they have signed in with Google.
        /// Playground at: https://developers.google.com/oauthplayground.
        /// </summary>
        /// <param name="authenticateSocialUserDto"></param>
        /// <returns>The user access token.</returns>
        [HttpPost]
        [Route("Google")]
        public async Task<IActionResult> AuthenticateGoogleUser([FromBody] AuthenticateSocialUserDTO authenticateSocialUserDto)
        {
            var googleUser = await _googleService.GetUserFromTokenAsync(authenticateSocialUserDto.Token);
            if (googleUser?.Id is null ||
                googleUser.Email is null ||
                googleUser.FirstName is null ||
                googleUser.LastName is null)
            {
                return NotFound(new ApiError(
                    $"Could not find a Google user from the given token. Token: {authenticateSocialUserDto.Token}"));
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == googleUser.Email &&
                                                          u.SocialLinks
                                                              .Any(x => x.PlatformId == googleUser.Id &&
                                                                        x.Platform == ExternalAuthPlatform.Google));

            return user == null
                ? await CreateExternalAuthUser(
                    googleUser,
                    googleUser.FirstName,
                    googleUser.LastName,
                    ExternalAuthPlatform.Google)
                : Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }

        /// <summary>
        /// Creates a user with an email and password.
        /// </summary>
        /// <param name="createUserDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateUserAsync(CreateUserDTO createUserDto)
        {
            var identityUser = new ApplicationUser
            {
                Email = createUserDto.Email,
                UserName = createUserDto.Email,
            };

            ApplicationUser user;

            try
            {
                await _userCustomerService.AddCustomerIdAsync(identityUser);
                var result = await _userManager.CreateAsync(identityUser, createUserDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(error => error.Code, error => error.Description);
                    return BadRequest(new ApiError("User creation failed.", errors));
                }

                user = await _userManager.FindByEmailAsync(identityUser.Email) ??
                       throw new Exception("Error fetching newly created user");
            }
            catch (Exception e)
            {
                Log.Error($"Error during user creation: Exception: {e}");
                await _userManager.DeleteAsync(identityUser);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiError($"Something went wrong during user creation: Exception: {e}"));
            }

            await _emailService.SendWelcomeEmail(user.Email);
            _telemetryClient.TrackEvent("New account created with email");

            return Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }

        private async Task<IActionResult> CreateExternalAuthUser(
            string id,
            string email,
            string firstName,
            string lastName,
            ExternalAuthPlatform platform)
        {
            var identityUser = new ApplicationUser
            {
                SocialLinks = new List<ExternalAuthLink>
                {
                    new ()
                    {
                        PlatformId = id,
                        Platform = platform,
                    },
                },
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
            };

            ApplicationUser user;

            try
            {
                await _userCustomerService.AddCustomerIdAsync(identityUser);
                var result = await _userManager.CreateAsync(identityUser);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(error => error.Code, error => error.Description);
                    return BadRequest(new ApiError($"{platform} user creation failed.", errors));
                }

                user = await _userManager.FindByEmailAsync(identityUser.Email) ??
                       throw new Exception("Error fetching newly created user");
            }
            catch (Exception e)
            {
                Log.Error($"Error during user creation: Exception: {e}");
                await _userManager.DeleteAsync(identityUser);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiError($"Something went wrong during user creation: Exception: {e}"));
            }

            // Do i need this?
            // await _context.SaveChangesAsync();
            await _emailService.SendWelcomeEmail(user.Email!);
            _telemetryClient.TrackEvent($"New account created with {platform}");

            return Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }
    }
}