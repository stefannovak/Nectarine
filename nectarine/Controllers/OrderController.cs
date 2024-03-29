using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.DTOs.Requests.Orders;
using NectarineAPI.Models;
using NectarineAPI.Models.Customers;
using NectarineAPI.Services;
using NectarineAPI.Services.Messaging;
using NectarineData.DataAccess;
using NectarineData.Models;
using SendGrid.Helpers.Mail;

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
    private readonly IUserCustomerService _userCustomerService;

    public OrderController(
        NectarineDbContext context,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IEmailService emailService,
        IPaymentService paymentService,
        IUserCustomerService userCustomerService)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
        _emailService = emailService;
        _paymentService = paymentService;
        _userCustomerService = userCustomerService;
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
        if (paymentMethod is null || paymentMethod.CustomerId != user.PaymentProviderCustomerId)
        {
            return BadRequest(new ApiError(
                $"The payment method ID: {createOrderDto.PaymentMethodId} does not correspond to this user."
            ));
        }

        var customer = _userCustomerService.GetCustomer(user.PaymentProviderCustomerId);
        if (customer is null)
        {
            return BadRequest(new ApiError("Could not get the Users customer information."));
        }

        var address = customer.Address;
        var order = new Order
        {
            User = user,
            ProductIds = createOrderDto.ProductIds,
            OrderTotal = createOrderDto.OrderTotal,
            PaymentMethodId = paymentMethod.Id,
            Postcode = address.Postcode,
        };

        _context.Orders.Add(order);
        await SendOrderConfirmationEmail(user.Email, order, paymentMethod.LastFour, address);
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
            return NotFound(new ApiError("Could not find an order with the given Id for this user."));
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
        return Ok(_mapper.Map<IList<OrderDTO>>(orders));
    }

    /// <summary>
    /// Cancel an order. Does not delete the order.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelOrder([FromQuery] string orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var order = _context.Orders.FirstOrDefault(x => x.Id == Guid.Parse(orderId) && x.User.Id == user.Id);
        if (order is null)
        {
            return NotFound(new ApiError("Could not find an order with the given Id for this user."));
        }

        order.IsCancelled = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    private async Task SendOrderConfirmationEmail(
        string destinationEmail,
        Order order,
        string lastFourCardNumber,
        UserAddress address)
    {
        var textContent = $"Thanks for your order {destinationEmail}!\n" +
                          "Your order has been created and will be dispatched soon.\n" +
                          $"Order Confirmation Number: {order.Id.ToString()}\n\n" +
                          $"Your order will be sent to {address.Line1} {address.Postcode}.\n" +
                          $"Order Total: {order.OrderTotal}\n" +
                          $"Payment method ending in: {lastFourCardNumber}\n\n" +
                          "We hope to see you again soon.\n" +
                          "Nectarine";

        await _emailService.SendEmail(
            destinationEmail,
            "Your Nectarine order receipt",
            textContent);
    }
}