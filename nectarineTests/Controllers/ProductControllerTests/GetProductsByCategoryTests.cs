using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NectarineAPI.Models.Products;
using Xunit;

namespace NectarineTests.Controllers.ProductControllerTests;

public partial class ProductControllerTests
{
    [Fact(DisplayName = "GetProductsByCategory should return OK")]
    public void Test_GetProductsByCategory_ReturnsOk()
    {
        // Act
        var result = _subject.GetProductsByCategory("shirt");

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetProductsByCategory should return BadRequest")]
    public void Test_GetProductsByCategory_ReturnsBadRequestWhen_NoProductsExist()
    {
        // Act
        var result = _subject.GetProductsByCategory("chaqueta");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "GetProductsByCategory should return BadRequest when Invalid OrderBy")]
    public void Test_GetProductsByCategory_ReturnsBadRequestWhen_InvalidOrderBy()
    {
        // Act
        var result = _subject.GetProductsByCategory("shirt", orderBy: "brand");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(DisplayName = "GetProductsByCategory should return a list of products in ascending order of price")]
    public void Test_GetProductsByCategory_ReturnsAscendingOrderOfPrice()
    {
        // Arrange
        var ascendingList = _products.Select(x => x.Price).OrderBy(x => x);

        // Act
        var result = _subject.GetProductsByCategory("shirt", descending: false, orderBy: "Price");

        // ! as if this is not correct, then there is a problem.
        var products = ((result as OkObjectResult) !.Value as GetProductsResponse) !.Products;
        var prices = products.Select(x => x.Price);

        // Assert
        Assert.True(ascendingList.SequenceEqual(prices));
    }
}