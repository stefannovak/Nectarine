using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using nectarineAPI.DTOs.Requests;
using nectarineAPI.Models;
using nectarineAPI.Services;
using nectarineData.DataAccess;

namespace nectarineAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly NectarineDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IUserCustomerService _userCustomerService;

        public PaymentController(
            NectarineDbContext context,
            IPaymentService paymentService,
            IUserCustomerService userCustomerService)
        {
            _context = context;
            _paymentService = paymentService;
            _userCustomerService = userCustomerService;
        }
        
        [HttpPost("AddPaymentMethod")]
        public ActionResult AddPaymentMethod(Guid userId, AddPaymentMethodDto addPaymentMethodDto)
        {
            var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            if (user is null)
            {
                return NotFound(new ApiError { Message = "Could not find a user with the given ID." });
            }
            
            _paymentService.AddCardPaymentMethod(
                user,
                addPaymentMethodDto.CardNumber,
                addPaymentMethodDto.ExpiryMonth,
                addPaymentMethodDto.ExpiryYear,
                addPaymentMethodDto.CVC);

            return Ok();
        }
    }
}