using Catalog.Application.Contracts;
using Catalog.Application.Interfaces;

namespace Catalog.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var productGroup = app.MapGroup("/api/products").WithTags("Products");

        productGroup.MapGet("/search", async (IProductSearchService productSearchService,
            string? queryParam,
            string? category,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortPrice,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default) =>
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

            var result = await productSearchService.SearchAsync(request, cancellationToken);
            return Results.Ok(result);
        });
    }
}