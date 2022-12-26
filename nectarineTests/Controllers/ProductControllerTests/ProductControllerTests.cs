using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using NectarineAPI.Controllers;
using NectarineAPI.DTOs.Generic;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineTests.Controllers.ProductControllerTests;

public partial class ProductControllerTests
{
    private readonly ProductController _subject;
    private readonly NectarineDbContext _context;
    private readonly List<Product> _products = new ()
    {
        new Product(
            name: "Black shirt",
            description: "What an amazing shirt",
            primaryColorHex: "#121122",
            primaryColorName: "pink",
            size: "L",
            price: 1000,
            material: "lace",
            sex: "M",
            category: "shirt",
            image: "https://www.uniqlo.com/jp/ja/contents/feature/masterpiece/common_22ss/img/products/contentsArea_itemimg_16.jpg"),
        new Product(
            name: "Black shirt",
            description: "What an amazing shirt",
            primaryColorHex: "#121122",
            primaryColorName: "pink",
            size: "L",
            price: 5000,
            material: "lace",
            sex: "M",
            category: "shirt",
            image: "https://www.uniqlo.com/jp/ja/contents/feature/masterpiece/common_22ss/img/products/contentsArea_itemimg_16.jpg"),
        new Product(
            name: "Black shirt",
            description: "What an amazing shirt",
            primaryColorHex: "#121122",
            primaryColorName: "pink",
            size: "L",
            price: 10000,
            material: "lace",
            sex: "M",
            category: "shirt",
            image: "https://www.uniqlo.com/jp/ja/contents/feature/masterpiece/common_22ss/img/products/contentsArea_itemimg_16.jpg"),
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

        // Mapper setup
        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(x => x.Map<IList<ProductDTO>>(It.IsAny<List<Product>>()))
            .Returns(new List<ProductDTO>
            {
                new () { Price = 1000 },
                new () { Price = 5000 },
                new () { Price = 10000 },
            });

        // Subject setup
        _subject = new ProductController(_context, mapperMock.Object);
    }
}