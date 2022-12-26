using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.DTOs.Generic;
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
    private readonly IMapper _mapper;

    public ProductController(NectarineDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get a paginated list of product per category.
    /// </summary>
    /// <param name="category">See supported categories.</param>
    /// <param name="descending"></param>
    /// <param name="orderBy">A property of a product.</param>
    /// <param name="pageSize">How many items you want to retrieve.</param>
    /// <param name="pageNumber">Calculated by total count of product for category / page size.</param>
    /// <returns>A list of Products, alongside a Pagination object.</returns>
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

        return Ok(new GetProductsResponse(
            _mapper.Map<IList<ProductDTO>>(products),
            new Pagination(hasMore, pageNumber, pageSize, totalCount)));
    }
}