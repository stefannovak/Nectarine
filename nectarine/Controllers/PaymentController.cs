using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineData.DataAccess;
using Stripe;

namespace nectarineAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly NectarineDbContext _context;
        private readonly IPaymentService _paymentService;

        public PaymentController(
            NectarineDbContext context,
            IPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }
        
        /// <summary>
        /// Creates a <see cref="PaymentMethod"/> and attaches it the user's <see cref="Customer"/> object.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="addPaymentMethodDto">A customers card details.</param>
        /// <returns></returns>
        [HttpPost("AddPaymentMethod")]
        public ActionResult AddPaymentMethod(String userId, AddPaymentMethodDTO addPaymentMethodDto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user is null)
            {
                return NotFound(new ApiError { Message = "Could not find a user with the given ID." });
            }

            var result = _paymentService.AddCardPaymentMethod(
                user,
                addPaymentMethodDto.CardNumber,
                addPaymentMethodDto.ExpiryMonth,
                addPaymentMethodDto.ExpiryYear,
                addPaymentMethodDto.CVC);
            if (result is not null)
            {
                return BadRequest(new ApiError
                {
                    Message = "Could not create the payment method.",
                    Errors =
                    {
                        new KeyValuePair<string, string>("Error", result.Message)
                    }
                });
            }

            return Ok();
        }
    }
}