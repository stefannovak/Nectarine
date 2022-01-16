using System.Collections.Generic;

namespace NectarineAPI.DTOs.Requests.Orders;

public class CreateOrderDto
{
    public List<string> ProductIds { get; set; } = new ();
}