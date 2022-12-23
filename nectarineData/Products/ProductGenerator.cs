using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NectarineData.DataAccess;
using NectarineData.Models;

namespace NectarineData.Products;

public class ProductGenerator
{
    private readonly Random _random = new ();

    private readonly IList<string> _supportedColours = new List<string>
    {
        "red",
        "orange",
        "yellow",
        "green",
        "blue",
        "indigo",
        "violet",
        "purple",
        "pink",
        "silver",
        "gold",
        "beige",
        "brown",
        "gray",
        "black",
        "white",
    };

    private readonly IList<string> _supportedSizes = new List<string>
    {
        "XXS",
        "XS",
        "S",
        "M",
        "L",
        "XL",
        "XXL",
        "XXXL",
    };

    private readonly IList<string> _supportedCategories = new List<string>
    {
        "accessory",
        "shirt",
        "t-shirt",
        "vest",
        "jumper",
        "coat",
        "dress",
        "blouse",
        "bra",
        "pants",
        "trousers",
        "skirt",
        "shorts",
        "socks",
        "shoes",
    };

    private readonly IList<string> _supportedMaterials = new List<string>
    {
        "Chiffon",
        "Cotton",
        "Crepe",
        "Denim",
        "Lace",
        "Leather",
        "Linen",
        "Satin",
    };

    public async Task GenerateAndSeedProducts(string connectionString)
    {
        var optionBuilder = new DbContextOptionsBuilder<NectarineDbContext>();
        optionBuilder.UseSqlServer(connectionString);
        var context = new NectarineDbContext(optionBuilder.Options);

        if (context.Products.Any())
        {
            return;
        }

        await context.Database.EnsureCreatedAsync();

        var products = GenerateBras();

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }

    private IList<Product> GenerateBras()
    {
        var bras = new List<Product>();

        for (var i = 0; i < _random.NextInt64(50, 100); i++)
        {
            var colour = GetRandomSupportedAttribue(_supportedColours);
            var size = GetRandomSupportedAttribue(_supportedSizes);
            var price = _random.NextInt64(10000);
            var material = GetRandomSupportedAttribue(_supportedMaterials);
            // TODO: - This should reflect the colour string
            var hex = GetRandomHexNumber();

            for (var j = 0; j <= _random.NextInt64(10); j++)
            {
                bras.Add(new Product(
                    name: $"{colour} Bra",
                    description: $"What an amazing {size} {colour} bra. Buy it.",
                    primaryColorHex: hex,
                    primaryColorName: colour,
                    size: size,
                    price: price,
                    material: material,
                    sex: "F",
                    category: "bra",
                    image: "https://img01.ztat.net/article/spp-media-p1/9ba2989a99c74d7b948839374beec9ca/93bdf0a695a344e8a822b7018f692c43.jpg?imwidth=1800&filter=packshot"));
            }
        }

        return bras;
    }

    private string GetRandomSupportedAttribue(IList<string> supportedAttribute)
        => supportedAttribute[_random.Next(supportedAttribute.Count)];

    private string GetRandomHexNumber() => $"#{_random.Next(0x1000000):X6}";

}