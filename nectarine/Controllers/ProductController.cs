using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.Models;
using NectarineAPI.Models.Products;
using NectarineData.DataAccess;

namespace NectarineAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly NectarineDbContext _context;

    public ProductController(NectarineDbContext context)
    {
        _context = context;
    }

    [HttpGet("{category}")]
    public async Task<IActionResult> GetProductsByCategory(string category)
    {
        var products = _context.Products
            .Where(p => string.Equals(p.Category, category))
            .Take(50);

        if (!products.Any())
        {
            return BadRequest(new ApiError($"No products for {category}"));
        }

        var hasMore = _context.Products
            .Where(p => string.Equals(p.Category, category))
            .Skip(50)
            .Take(1)
            .Any();

        var totalCount = _context.Products.Count();

        return Ok(new GetProductsResponse(products, new Pagination(hasMore, 0, 50, totalCount)));
    }

    [HttpGet("{category}/{pageSize}/{pageNumber}")]
    public async Task<IActionResult> GetProductsByCategoryPaginated(string category, int? pageSize, int? pageNumber)
    {
        return Ok();
    }
}