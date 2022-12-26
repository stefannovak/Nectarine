using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NectarineAPI.Controllers;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineTests.Controllers.ProductControllerTests;

public partial class ProductControllerTests
{
    private readonly ProductController _subject;
    private readonly NectarineDbContext _context;
    private readonly List<Product> _products = new ()
    {
        new ()
        {
            Name = "Black shirt",
            Category = "shirt",
            Price = 1000,
        },
        new ()
        {
            Name = "Blue shirt",
            Category = "shirt",
            Price = 5000,
        },
        new ()
        {
            Name = "Grey shirt",
            Category = "shirt",
            Price = 10000,
        },
    };

    public ProductControllerTests()
    {
        // Database setup
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _context = new NectarineDbContext(options);
        if (!_context.Products.Any())
        {
            _products.ForEach(p => _context.Add(p));
            _context.SaveChanges();
        }

        // Subject setup
        _subject = new ProductController(_context);
    }
}