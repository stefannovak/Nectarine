using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Requests.Orders;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineAPI.Controllers;

public class OrderController : ControllerBase
{
    private readonly NectarineDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(NectarineDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateOrder(CreateOrderDto createOrderDto)
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
}