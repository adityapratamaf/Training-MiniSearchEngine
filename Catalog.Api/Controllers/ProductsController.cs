using Catalog.Application.Contracts;
using Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductSearchService _productSearchService;

    public ProductsController(IProductSearchService productSearchService)
    {
        _productSearchService = productSearchService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? queryParam,
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var request = new ProductSearchRequest
        {
            QueryParam = queryParam,
            Category = category,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            SortPrice = sortPrice,
            Page = page,
            PageSize = pageSize
        };

        var result = await _productSearchService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }
}