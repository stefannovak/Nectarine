using System.Collections.Generic;
using System.Linq;
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
        /// 4242424242424242 is the test stripe card number.
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

            var success = _paymentService.AddCardPaymentMethod(
                user.PaymentProviderCustomerId,
                addPaymentMethodDto.CardNumber,
                addPaymentMethodDto.ExpiryMonth,
                addPaymentMethodDto.ExpiryYear,
                addPaymentMethodDto.CVC);

            return success
                ? Ok()
                : BadRequest(new ApiError
                {
                    Message = "Could not create the payment method.",
                });
        }

        /// <summary>
        /// Get an User's payment method with an Id.
        /// </summary>
        /// <param name="paymentMethodId"></param>
        /// <returns></returns>
        [HttpGet("GetPaymentMethod")]
        public async Task<IActionResult> GetPaymentMethod([FromQuery] string paymentMethodId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var paymentMethod = _paymentService.GetPaymentMethod(paymentMethodId);
            if (paymentMethod is null || user.PaymentProviderCustomerId != paymentMethod.CustomerId)
            {
                return NotFound(new ApiError { Message = "Could not find a Payment Method with the given Id for this user." });
            }

            return Ok(new { PaymentMethod = paymentMethod });
        }

        // Get all payment methods
        /// <summary>
        /// Get a Users payment methods.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllPaymentMethods")]
        public async Task<IActionResult> GetPaymentMethod()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var paymentMethods = _paymentService.GetCardsForUser(user);
            if (!paymentMethods.Any())
            {
                return NotFound(new ApiError { Message = "Could not find any payment methods for this user." });
            }

            return Ok(new
            {
                PaymentMethods = paymentMethods,
            });
        }
    }
}