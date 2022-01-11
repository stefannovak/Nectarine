using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.DTOs.Responses;
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineAPI.Services.Auth;
using nectarineData.DataAccess;
using nectarineData.Models;
using nectarineData.Models.Enums;

namespace nectarineAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IExternalAuthService<GoogleUser> _googleService;
        private readonly IExternalAuthService<MicrosoftUser> _microsoftService;
        private readonly IUserCustomerService _userCustomerService;
        private readonly NectarineDbContext _context;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IExternalAuthService<GoogleUser> googleService,
            IExternalAuthService<MicrosoftUser> microsoftService,
            IUserCustomerService userCustomerService,
            NectarineDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _googleService = googleService;
            _microsoftService = microsoftService;
            _userCustomerService = userCustomerService;
            _context = context;
        }
        
        [HttpPost]
        [Route("Authenticate")]
        public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticateUserDTO authenticateUserDto)
        {
            var user = await _userManager.FindByEmailAsync(authenticateUserDto.Email);
            if (user is null)
            {
                return Unauthorized(new ApiError { Message = "Authentication failed. User not found." });
            }

            var passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                authenticateUserDto.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new ApiError { Message = "Authentication failed. Incorrect password." });
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
                return NotFound(new ApiError { Message = "Could not find a Microsoft user from the given token." +
                                                         $" Token: {authenticateSocialUserDto.Token}"});
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == microsoftUser.Email &&
                                                          u.SocialLinks
                                                              .Any(x => x.PlatformId == microsoftUser.Id && 
                                                                        x.Platform == ExternalAuthPlatform.Microsoft));

            return user == null 
                ? await CreateExternalAuthUser(microsoftUser, ExternalAuthPlatform.Microsoft)
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
                googleUser.FirstName is null ||
                googleUser.LastName is null)
            {
                return NotFound(new ApiError { Message = "Could not find a Google user from the given token." +
                                                         $" Token: {authenticateSocialUserDto.Token}"});
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == googleUser.Email &&
                                                          u.SocialLinks
                                                              .Any(x => x.PlatformId == googleUser.Id && 
                                                                        x.Platform == ExternalAuthPlatform.Google));

            return user == null 
                ? await CreateExternalAuthUser(googleUser, ExternalAuthPlatform.Google)
                : Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }

        private async Task<IActionResult> CreateExternalAuthUser(IExternalAuthUser externalUser, ExternalAuthPlatform platform)
        {
            var user = new ApplicationUser
            {
                SocialLinks = new List<SocialLink>
                {
                    new()
                    {
                        PlatformId = externalUser.Id!,
                        Platform = platform,
                    }
                },
                Email = externalUser.Email,
                UserName = externalUser.Email,
                FirstName = externalUser.FirstName!,
                LastName = externalUser.LastName!,
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return Problem($"{externalUser.Platform} user creation failed. Possible user email duplication.");
            }

            await _userCustomerService.AddStripeCustomerIdAsync(user);
            await _context.SaveChangesAsync();

            return Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }
    }
}