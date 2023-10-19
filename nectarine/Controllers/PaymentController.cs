using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.Models;
using NectarineAPI.Services;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineAPI.Controllers
{
    [Authorize]
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
                return NotFound(new ApiError("Could not find a Payment Method with the given Id for this user."));
            }

            return Ok(new { PaymentMethod = paymentMethod });
        }

        // Get all payment methods
        /// <summary>
        /// Get a Users payment methods.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllPaymentMethods")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var paymentMethods = _paymentService.GetCardsForUser(user.PaymentProviderCustomerId);
            if (!paymentMethods.Any())
            {
                return NotFound(new ApiError("Could not find any payment methods for this user."));
            }

            return Ok(paymentMethods);
        }
    }
}