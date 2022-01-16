using System.Collections.Generic;

namespace NectarineAPI.DTOs.Requests.Orders;

public class CreateOrderDTO
{
    public List<string> ProductIds { get; set; } = new ();
}