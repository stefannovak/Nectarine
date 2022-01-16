using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Requests;
using NectarineAPI.Models;
using NectarineAPI.Services;
using NectarineData.DataAccess;
using NectarineData.Models;
using Stripe;

namespace NectarineAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly NectarineDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(
            NectarineDbContext context,
            IPaymentService paymentService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _paymentService = paymentService;
            _userManager = userManager;
        }

        /// <summary>
        /// Creates a <see cref="PaymentMethod"/> and attaches it the user's <see cref="Customer"/> object.
        /// </summary>
        /// <param name="addPaymentMethodDto">A customers card details.</param>
        /// <returns></returns>
        [HttpPost("AddPaymentMethod")]
        public async Task<IActionResult> AddPaymentMethod(AddPaymentMethodDTO addPaymentMethodDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
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
                        new KeyValuePair<string, string>("Error", result.Message),
                    },
                });
            }

            return Ok();
        }
    }
}