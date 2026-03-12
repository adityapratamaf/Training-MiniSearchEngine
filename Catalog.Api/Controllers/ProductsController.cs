using Catalog.Application.Contracts;
using Catalog.Application.Interfaces;

namespace Catalog.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var productGroup = app.MapGroup("/api/products").WithTags("Products");

        // Search
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

        // Create
        productGroup.MapPost("/", async (IProductCommandService productCommandService, CreateProductRequest request, CancellationToken cancellationToken) =>
        {
            var result = await productCommandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/products/{result.Id}", result);
        });

        // Edit
        productGroup.MapPut("/{id:guid}", async (IProductCommandService productCommandService, Guid id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        {
            request.Id = id;
            var result = await productCommandService.UpdateAsync(request, cancellationToken);
            return Results.Ok(result);
        });

        // Delete
        productGroup.MapDelete("/{id:guid}", async (IProductCommandService productCommandService, Guid id, CancellationToken cancellationToken) =>
        {
            await productCommandService.DeleteAsync(id, cancellationToken);
            return Results.NoContent();
        });

    }
}