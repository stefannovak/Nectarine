using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
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
        public IActionResult GetCurrentAsync()
        {
            var userId = _userManager.GetUserId(User);
            var user = _context.Users
                .Include(x => x.SubmittedRatings)
                .Include(x => x.UserAddresses)
                .FirstOrDefault(x => x.Id == userId);

            if (user is null)
            {
                return BadRequest(new ApiError("Could not get a user"));
            }

            return Ok(_mapper.Map<UserDTO>(user));
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
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
        /// <param name="updatePhoneNumberDto"></param>
        /// <returns></returns>
        [HttpPost("Update/PhoneNumber")]
        public async Task<IActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberDTO updatePhoneNumberDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            user.PhoneNumber = updatePhoneNumberDto.PhoneNumber;
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