using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nectarineAPI.DTOs.Generic;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.DTOs.Responses;
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineData.DataAccess;
using nectarineData.Models;
using Stripe;

namespace nectarineAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserCustomerService _userCustomerService;
        private readonly NectarineDbContext _context;
        private readonly ITokenService _tokenService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IUserCustomerService userCustomerService,
            NectarineDbContext context,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userCustomerService = userCustomerService;
            _context = context;
            _tokenService = tokenService;
        }
        
        /// <summary>
        /// Returns the current user from their <see cref="ControllerBase"/> Claims Principle.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCurrent")]
        public async Task<IActionResult> GetCurrentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return BadRequest(new ApiError { Message = "Could not get a user" });
            }

            return Ok(_mapper.Map<UserDTO>(user));
        }

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


            var result = await _userManager.CreateAsync(identityUser, createUserDto.Password);

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

            return Ok(new CreateUserResponse(_tokenService.GenerateTokenAsync(user)));
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return BadRequest(new ApiError { Message = "Could not get a user" });
            }

            var applicationUser = _context.Users.FirstOrDefault(x => x.Id == user.Id);
            if (applicationUser is null)
            {
                return BadRequest(new ApiError { Message = "Could not get a user from the database" });
            }

            var result = _userCustomerService.DeleteCustomer(applicationUser);
            if (!result)
            {
                return BadRequest(new ApiError { Message = "This user does not have a Customer associate with it." });
            }

            _context.Users.Remove(applicationUser);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        [Route("Update")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")] // They CAN be null.
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserDTO updateUserDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return BadRequest(new ApiError { Message = "Could not get a user" });
            }

            var customer = _userCustomerService.GetCustomer(user);
            if (customer is null)
            {
                return BadRequest(new ApiError { Message = "Could not get a customer from the user" });
            }

            _userCustomerService.UpdateCustomer(user, new CustomerUpdateOptions
            {
                Email = updateUserDto.Email,
                Address = updateUserDto.Address,
            });

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<UserDTO>(user));
        }
    }
}