using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineData.Models;

namespace nectarineAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
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
    }
}