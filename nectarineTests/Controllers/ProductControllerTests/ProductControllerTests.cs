using Microsoft.EntityFrameworkCore;
using NectarineAPI.Controllers;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineTests.Controllers.ProductControllerTests;

public partial class ProductControllerTests
{
    private readonly ProductController _subject;
    private readonly NectarineDbContext _context;

    public ProductControllerTests()
    {
        // Database setup
        var options = new DbContextOptionsBuilder<NectarineDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _context = new NectarineDbContext(options);
        _context.Products.Add(new Product
        {
            Name = "Black shirt",
            Category = "shirt",
        });

        _context.Products.Add(new Product
        {
            Name = "Blue shirt",
            Category = "shirt",
        });

        _context.SaveChanges();

        // Subject setup
        _subject = new ProductController(_context);
    }
}