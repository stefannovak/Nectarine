using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Models;
using NectarineAPI.Models.Customers;
using NectarineAPI.Services;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using Serilog;

namespace NectarineAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserCustomerService _userCustomerService;
        private readonly NectarineDbContext _context;
        private readonly IPhoneService _phoneService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IUserCustomerService userCustomerService,
            NectarineDbContext context,
            IPhoneService phoneService,
            IBackgroundJobClient backgroundJobClient)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userCustomerService = userCustomerService;
            _context = context;
            _phoneService = phoneService;
            _backgroundJobClient = backgroundJobClient;
        }

        /// <summary>
        /// Returns the current user from their <see cref="ControllerBase"/> Claims Principle.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCurrent")]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OkObjectResult), StatusCodes.Status200OK)]
        public IActionResult GetCurrentAsync()
        {
            var userId = _userManager.GetUserId(User);
            var user = _context.Users
                .Include(x => x.SubmittedRatings)
                .Include(x => x.UserAddresses)
                .FirstOrDefault(x => x.Id == userId);

            if (user is null)
            {
                return Unauthorized();
            }

            return Ok(_mapper.Map<UserDTO>(user));
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAsync()
        {
            var userId = _userManager.GetUserId(User);
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            if (user is null)
            {
                return Unauthorized();
            }

            _userCustomerService.DeleteCustomer(user);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Update the users phone number.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Update/PhoneNumber")]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberDTO request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            user.PhoneNumber = request.PhoneNumber;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new ApiError("Failed to update phone number on database"));
            }

            return NoContent();
        }

        /// <summary>
        /// Send a verification code to a users phone number.
        /// </summary>
        /// <returns></returns>
        [HttpGet("VerificationCode")]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetVerificationCode()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                return BadRequest(new ApiError("User does not have a phone number."));
            }

            var random = new Random();
            var verificationCode = random.Next(100000, 999999);

            _phoneService.SendMessage(
                $"Your verification code is {verificationCode}. This code will expire in 2 minutes.",
                user.PhoneNumber);

            user.VerificationCode = verificationCode;
            await _userManager.UpdateAsync(user);
            _backgroundJobClient.Schedule(() => DeleteVerificationCodeForUser(user.Id), TimeSpan.FromMinutes(2));

            return NoContent();
        }

        /// <summary>
        /// Confirm a users phone number with a verification code.
        /// </summary>
        /// <param name="confirm2FaCodeDto"></param>
        /// <returns></returns>
        [HttpPost("Confirm2FACode")]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Confirm2FACode(Confirm2FACodeDTO confirm2FaCodeDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            if (user.VerificationCode is null)
            {
                return BadRequest(new ApiError("User does not have a valid verification code."));
            }

            if (user.VerificationCode != confirm2FaCodeDto.Code)
            {
                return BadRequest(new ApiError("Invalid code."));
            }

            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);
            return NoContent();
        }

        // ReSharper disable once MemberCanBePrivate.Global - Hangfire requires public
        [NonAction]
        public async Task DeleteVerificationCodeForUser(string userId)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            if (user is null)
            {
                Log.Debug($"Failed to find user with id {userId}");
                return;
            }

            user.VerificationCode = null;
            await _context.SaveChangesAsync();
        }
    }
}