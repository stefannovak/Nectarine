using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.Models.Customers;

public class UserAddress
{
    public UserAddress(
        string line1,
        string? line2,
        string city,
        string postcode,
        string country,
        bool isPrimaryAddress)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        Postcode = postcode;
        Country = country;
        IsPrimaryAddress = isPrimaryAddress;
    }

    [Required]
    [MaxLength(100)]
    public string Line1 { get; set; }

    [MaxLength(100)]
    public string? Line2 { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; }

    [Required]
    [MaxLength(10)]
    public string Postcode { get; set; }
    
    /// <summary>
    /// Two-letter country code (ISO 3166-1 alpha-2).
    /// </summary>
    [Required]
    [MaxLength(2)]
    public string Country { get; set; }

    [Required]
    public bool IsPrimaryAddress { get; set; }
}