using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NectarineData.Models;

public class Order
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    public virtual ApplicationUser User { get; set; } = null!;

    [Required]
    public virtual ICollection<string> ProductIds { get; set; } = new List<string>();

    public bool IsCancelled { get; set; }

    public bool IsFulfilled { get; set; }

    public string? PaymentMethodId { get; set; }

    public string? PaymentMethod { get; set; }

    [Required]
    public string OrderTotal { get; set; } = string.Empty;

    [Required]
    public string Postcode { get; set; } = string.Empty;
}