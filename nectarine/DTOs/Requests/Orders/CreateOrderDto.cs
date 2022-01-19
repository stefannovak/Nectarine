using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NectarineAPI.DTOs.Requests.Orders;

public class CreateOrderDTO
{
    [Required]
    public List<string> ProductIds { get; set; } = new ();

    [Required]
    public string OrderTotal { get; set; } = null!;

    [Required]
    public string PaymentMethodId { get; set; } = null!;
}