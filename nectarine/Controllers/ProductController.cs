using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.Models;
using NectarineAPI.Models.Products;
using NectarineData.DataAccess;
using NectarineData.Models;

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
    public IActionResult GetProductsByCategory(
        string category,
        [FromQuery]
        bool descending = true,
        string orderBy = "Price",
        int pageSize = 50,
        int pageNumber = 0)
    {
        var prop = typeof(Product)
            .GetProperties()
            .FirstOrDefault(x => string.Equals(x.Name, orderBy, StringComparison.CurrentCultureIgnoreCase))?
            .Name;

        if (prop is null)
        {
            return BadRequest(new ApiError(
                $"Unsupported orderBy property: '{orderBy}'. Valid properties: " +
                $"{string.Join(", ", typeof(Product).GetProperties().Select(x => x.Name))}"));
        }

        var productsForCategory = _context.Products
            .Where(p => string.Equals(p.Category, category));

        var products = productsForCategory
            .AsEnumerable()
            .OrderBy(x => x.GetType().GetProperty(prop)?.GetValue(x, null))
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToList();

        if (!products.Any())
        {
            return BadRequest(new ApiError($"No products for {category}"));
        }

        if (descending)
        {
            products.Reverse();
        }

        var totalCount = ((int)Math.Ceiling((double)productsForCategory.Count() / pageSize)) - 1;
        var hasMore = pageNumber < totalCount;

        return Ok(new GetProductsResponse(products, new Pagination(hasMore, pageNumber, pageSize, totalCount)));
    }
}