using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NectarineData.Models;

public class Product
{
    public Product()
    {
    }

    public Product(
        string name,
        string description,
        string primaryColorHex,
        string primaryColorName,
        string size,
        double price,
        string material,
        string sex,
        string category,
        string image)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        PrimaryColorHex = primaryColorHex;
        PrimaryColorName = primaryColorName;
        Size = size;
        Price = price;
        Material = material;
        Sex = sex;
        Category = category;
        Image = image;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// #121212.
    /// </summary>
    [Required]
    [MaxLength(7)]
    public string PrimaryColorHex { get; set; } = string.Empty;

    /// <summary>
    /// red, orange, yellow, green, blue, indigo, violet, purple, pink, silver, gold, beige, brown, gray, black, white.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string PrimaryColorName { get; set; } = string.Empty;

    /// <summary>
    /// XXS, XS, S, M, L, XL, XXL, XXXL.
    /// </summary>
    [Required]
    [MaxLength(5)]
    public string Size { get; set; } = string.Empty;

    /// <summary>
    /// In lowest value for given currency. £1.00 = 100.
    /// </summary>
    [Required]
    public double Price { get; set; }

    [Required]
    [MaxLength(500)]
    public string Material { get; set; } = string.Empty;

    /// <summary>
    /// M, F, X.
    /// </summary>
    [Required]
    [MaxLength(1)]
    public string Sex { get; set; } = string.Empty;

    /// <summary>
    /// accessory, shirt, t-shirt, vest, jumper, coat, dress, blouse, bra, pants, trousers, skirt, shorts, socks, shoes.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Image { get; set; } = string.Empty;
}