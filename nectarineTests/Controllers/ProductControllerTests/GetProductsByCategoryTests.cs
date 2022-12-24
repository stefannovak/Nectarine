using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace NectarineTests.Controllers.ProductControllerTests;

public partial class ProductControllerTests
{
    [Fact(DisplayName = "GetProductsByCategory should return OK")]
    public async Task Test_GetProductsByCategory_ReturnsOk()
    {
        // Act
        var result = await _subject.GetProductsByCategory("shirt");

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact(DisplayName = "GetProductsByCategory should return BadRequest")]
    public async Task Test_GetProductsByCategory_ReturnsBadRequestWhen_NoProductsExist()
    {
        // Act
        var result = await _subject.GetProductsByCategory("chaqueta");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}