using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NectarineData.Models;

public class UserAddress
{
    /// <summary>
    /// Not to be used.
    /// </summary>
    public UserAddress()
    {
    }

    public UserAddress(
        string line1,
        string? line2,
        string city,
        string postcode,
        string country,
        bool isPrimaryAddress = false)
    {
        Id = Guid.NewGuid();
        Line1 = line1;
        Line2 = line2;
        City = city;
        Postcode = postcode;
        Country = country;
        IsPrimaryAddress = isPrimaryAddress;
    }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

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

    public virtual ApplicationUser ApplicationUser { get; set; } = null!;
}