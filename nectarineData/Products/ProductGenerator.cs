using System;
using System.Collections.Generic;
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

    private readonly IList<string> _supportedSexs = new List<string>
    {
        "M",
        "F",
        "X",
    };

    public IEnumerable<Product> GenerateProducts()
    {
        var products = new List<Product>();

        foreach (var category in _supportedCategories)
        {
            for (var i = 0; i < _random.NextInt64(50, 100); i++)
            {
                var colour = GetRandomSupportedAttribue(_supportedColours);
                var size = GetRandomSupportedAttribue(_supportedSizes);
                var price = _random.NextInt64(10000);
                var material = GetRandomSupportedAttribue(_supportedMaterials);

                for (var j = 0; j <= _random.NextInt64(10); j++)
                {
                    products.Add(new Product(
                        name: $"{colour} {category}",
                        description: $"What an amazing {size} {colour} {category}. Buy it.",
                        primaryColorName: colour,
                        size: size,
                        price: price,
                        material: material,
                        sex: category == "bra" ? "F" : GetRandomSupportedAttribue(_supportedSexs),
                        category: category,
                        // Probably host a bunch of images at a url that looks like nectarineImages/{category}/{sizes}
                        image: "https://www.uniqlo.com/jp/ja/contents/feature/masterpiece/common_22ss/img/products/contentsArea_itemimg_16.jpg"));
                }
            }
        }

        return products;
    }

    private string GetRandomSupportedAttribue(IList<string> supportedAttribute)
        => supportedAttribute[_random.Next(supportedAttribute.Count)];

    private string GetRandomHexNumber() => $"#{_random.Next(0x1000000):X6}";

}