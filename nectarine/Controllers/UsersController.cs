using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nectarineAPI.DTOs.Generic;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineData.Models;

namespace nectarineAPI.Controllers
{
    [Microsoft.AspNetCore.Components.Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserCustomerService _userCustomerService;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper, IUserCustomerService userCustomerService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userCustomerService = userCustomerService;
        }
        
        /// <summary>
        /// Returns the current user from their <see cref="ControllerBase"/> Claims Principle.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCurrent")]
        public async Task<IActionResult> GetCurrentAsync() => Ok(_mapper.Map<UserDTO>(await _userManager.GetUserAsync(User)));

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateUserAsync(CreateUserDTO createUserDto)
        {
            if (createUserDto.Email.IsNullOrEmpty() || createUserDto.Password.IsNullOrEmpty())
            {
                return BadRequest(new ApiError {Message = "Email or Password can not be null."});
            }

            var identityUser = new ApplicationUser
            {
                Email = createUserDto.Email,
                UserName = createUserDto.Email,
            };

            await _userCustomerService.AddStripeCustomerIdAsync(identityUser);


            var result = 
                await _userManager.CreateAsync(identityUser, createUserDto.Password);

            if (!result.Succeeded)
            {
                var dictionary = new Dictionary<string, string>();
                foreach (IdentityError error in result.Errors)
                {
                    dictionary.Add(error.Code, error.Description);
                }

                return new BadRequestObjectResult(new ApiError { Message = "User creation failed.", Errors = dictionary });
            }
            
            var user = await _userManager.FindByEmailAsync(identityUser.Email);
            if (user == null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiError() { Message = "Failed to retrieve new user" }
                    );
            }

            return Created($"/Users/{user.Id}", _mapper.Map<UserDTO>(user));
        }
    }
}