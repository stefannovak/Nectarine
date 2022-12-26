using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Generic;

public class ProductDTO
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? PrimaryColorHex { get; set; }

    public string? PrimaryColorName { get; set; }

    public string? Size { get; set; }

    public double Price { get; set; }

    public string? Material { get; set; }

    public string? Sex { get; set; }

    public string? Category { get; set; }

    public string? Image { get; set; }
}