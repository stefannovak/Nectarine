using System.Collections.Generic;
using NectarineAPI.DTOs.Generic;
using NectarineData.Models;

namespace NectarineAPI.Models.Products;

public class GetProductsResponse
{
    public GetProductsResponse(IList<ProductDTO> products, Pagination pagination)
    {
        Products = products;
        Pagination = pagination;
    }

    public IList<ProductDTO> Products { get; set; }

    public Pagination Pagination { get; set; }
}