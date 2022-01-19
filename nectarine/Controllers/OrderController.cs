using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.DTOs.Requests.Orders;
using NectarineAPI.Models;
using NectarineAPI.Services;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using SendGrid.Helpers.Mail;
using Stripe;
using Order = NectarineData.Models.Order;

namespace NectarineAPI.Controllers;

[Route("[controller]")]
[Authorize]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly NectarineDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;

    public OrderController(
        NectarineDbContext context,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IEmailService emailService,
        IPaymentService paymentService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _emailService = emailService;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Create an Order for a user.
    /// </summary>
    /// <param name="createOrderDto">List of productIds.</param>
    /// <returns></returns>
    [HttpPost("Create")]
    public async Task<IActionResult> CreateOrder(CreateOrderDTO createOrderDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var paymentMethod = _paymentService.GetPaymentMethod(createOrderDto.PaymentMethodId);
        if (paymentMethod is null || paymentMethod?.CustomerId != user.StripeCustomerId)
        {
            return BadRequest(new ApiError
            {
                Message = $"The payment method ID: {createOrderDto.PaymentMethodId} does not correspond to this user.",
            });
        }

        var order = new Order
        {
            User = user,
            ProductIds = createOrderDto.ProductIds,
            OrderTotal = createOrderDto.OrderTotal,
            PaymentMethod = paymentMethod.Id,
        };

        _context.Orders.Add(order);
        await SendOrderConfirmationEmail(user, order, paymentMethod);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            OrderId = order.Id.ToString(),
        });
    }

    /// <summary>
    /// Get an User's order with an Id.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpGet("Get")]
    public async Task<IActionResult> GetOrder([FromQuery] Guid orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var order = _context.Orders.FirstOrDefault(x => x.Id == orderId && x.User.Id == user.Id);
        if (order is null)
        {
            return NotFound(new ApiError { Message = "Could not find an order with the given Id for this user." });
        }

        return Ok(_mapper.Map<OrderDTO>(order));
    }

    /// <summary>
    /// Get a Users orders.
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var orders = _context.Orders.Where(x => x.User.Id == user.Id);
        if (!orders.Any())
        {
            return NotFound(new ApiError { Message = "Could not find any orders for this user." });
        }

        return Ok(_mapper.Map<IList<OrderDTO>>(orders));
    }

    /// <summary>
    /// Cancel an order. Does not delete the order.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelOrder([FromBody] string orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var order = _context.Orders.FirstOrDefault(x => x.Id == Guid.Parse(orderId) && x.User.Id == user.Id);
        if (order is null)
        {
            return NotFound(new ApiError { Message = "Could not find an order with the given Id for this user." });
        }

        order.IsCancelled = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    private async Task SendOrderConfirmationEmail(ApplicationUser user, Order order, PaymentMethod paymentMethod)
    {
        await _emailService.SendEmail(user.Email, new SendGridMessage
        {
            Subject = "Your Nectarine order receipt",
            PlainTextContent =
                $"Thanks for your order {user.FirstName}!\n" +
                "Your order has been created and will be dispatched soon.\n" +
                $"Order Confirmation Number: {order.Id.ToString()}\n\n" +
                $"Your order will be sent to .\n" +
                $"Order Total: {order.OrderTotal}\n" +
                $"Payment method ending in: {paymentMethod.Card.Last4}\n\n" +
                "We hope to see you again soon.\n" +
                "Nectarine",
        });
    }
}