using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.DTOs.Responses;
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineData.DataAccess;
using nectarineData.Models;
using nectarineData.Models.Enums;

namespace nectarineAPI.Controllers
{
    // [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ISocialService<GoogleUser> _googleService;
        private readonly NectarineDbContext _context;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            ISocialService<GoogleUser> googleService,
            NectarineDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _googleService = googleService;
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
                                                                        x.Platform == SocialPlatform.Google));

            return user == null 
                ? await CreateGoogleUser(googleUser)
                : Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }

        private async Task<IActionResult> CreateGoogleUser(GoogleUser googleUser)
        {
            var user = new ApplicationUser
            {
                SocialLinks = new List<SocialLink>
                {
                    new()
                    {
                        PlatformId = googleUser.Id!,
                        Platform = SocialPlatform.Google,
                    }
                },
                Email = googleUser.Email,
                UserName = googleUser.Email,
                FirstName = googleUser.FirstName!,
                LastName = googleUser.LastName!,
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiError { Message = "Google user creation failed" });
            }

            return Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }
    }
}