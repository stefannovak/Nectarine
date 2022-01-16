using System.Collections.Generic;

namespace NectarineAPI.DTOs.Generic;

public class OrderDTO
{
    public ICollection<string> ProductIds { get; set; } = new List<string>();

    public bool IsCancelled { get; set; }

    public bool IsFulfilled { get; set; }

    public string? PaymentMethodId { get; set; }

    public string? PaymentMethod { get; set; }

    public string OrderTotal { get; set; } = string.Empty;
}