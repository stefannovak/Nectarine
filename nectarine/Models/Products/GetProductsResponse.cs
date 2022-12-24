using System.Collections.Generic;
using NectarineData.Models;

namespace NectarineAPI.Models.Products;

public class GetProductsResponse
{
    public GetProductsResponse(IEnumerable<Product> products, Pagination pagination)
    {
        Products = products;
        Pagination = pagination;
    }

    public IEnumerable<Product> Products { get; set; }

    public Pagination Pagination { get; set; }
}