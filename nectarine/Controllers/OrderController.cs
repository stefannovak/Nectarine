using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Generic;
using NectarineAPI.DTOs.Requests.Orders;
using NectarineAPI.Models;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineAPI.Controllers;

[Route("[controller]")]
[Authorize]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly NectarineDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public OrderController(
        NectarineDbContext context,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
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

        var order = new Order
        {
            User = user,
            ProductIds = createOrderDto.ProductIds,
        };

        _context.Orders.Add(order);
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
}