using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NectarineData.Models;

public class UserAddress
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    public virtual ApplicationUser User { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Line1 { get; set; } = null!;

    [MaxLength(100)]
    public string? Line2 { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string Postcode { get; set; } = null!;
}