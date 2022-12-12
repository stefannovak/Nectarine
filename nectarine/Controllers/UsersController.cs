using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Models;
using NectarineAPI.Services;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using Stripe;

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

        public UsersController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IUserCustomerService userCustomerService,
            NectarineDbContext context,
            IPhoneService phoneService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userCustomerService = userCustomerService;
            _context = context;
            _phoneService = phoneService;
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

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Update the user's address.
        /// </summary>
        /// <param name="updateAddressDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse", Justification = "They can be null")]
        public async Task<IActionResult> UpdateAddressAsync([FromBody] UpdateAddressDTO updateAddressDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return BadRequest(new ApiError { Message = "Could not get a user" });
            }

            var customer = _userCustomerService.GetCustomer(user.PaymentProviderCustomerId);
            if (customer is null)
            {
                return BadRequest(new ApiError { Message = "Could not get a customer from the user" });
            }

            var addressId = Guid.NewGuid();
            _context.Add(new UserAddress
            {
                Id = addressId,
                User = user,
                Line1 = updateAddressDto.Address.Line1,
                Line2 = updateAddressDto.Address.Line2,
                City = updateAddressDto.Address.City,
                Postcode = updateAddressDto.Address.PostalCode,
            });

            if (updateAddressDto.IsPrimaryAddress)
            {
                _userCustomerService.UpdateCustomer(user, new CustomerUpdateOptions
                {
                    Address = updateAddressDto.Address,
                });
                user.CurrentShippingAddressId = addressId;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Update the users phone number.
        /// </summary>
        /// <param name="updatePhoneNumberDto"></param>
        /// <returns></returns>
        [HttpPost("UpdatePhoneNumber")]
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
                return BadRequest(new ApiError { Message = "Failed to update phone number" });
            }

            return Ok();
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
                return BadRequest(new ApiError { Message = "User does not have a phone number." });
            }

            var random = new Random();
            var verificationCode = random.Next(100000, 999999);

            _phoneService.SendMessage(
                $"Your verification code is {verificationCode}. This code will expire in 2 minutes.",
                user.PhoneNumber);

            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.Now.AddMinutes(2);
            await _userManager.UpdateAsync(user);

            return Ok();
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

            if (user.VerificationCodeExpiry is null ||
                user.VerificationCode is null)
            {
                return BadRequest(new ApiError { Message = "User did not get a verification code." });
            }

            if (user.VerificationCodeExpiry < DateTime.Now)
            {
                return BadRequest(new ApiError { Message = "Verification code expired." });
            }

            if (user.VerificationCode != confirm2FaCodeDto.Code)
            {
                return BadRequest(new ApiError { Message = "Invalid code." });
            }

            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);
            return Ok();
        }
    }
}