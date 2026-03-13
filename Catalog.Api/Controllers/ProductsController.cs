using Catalog.Application.Contracts;
using Catalog.Application.Interfaces;

namespace Catalog.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var productGroup = app.MapGroup("/api/products").WithTags("Products");

        // Search By Text
        productGroup.MapGet("/search-by-text", async (IProductSearchService productSearchService,
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

        // Search By Image
        productGroup.MapPost("/search-by-image", async (IFormFile image, IProductSearchService productSearchService,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default) =>
        {
            if (image is null || image.Length == 0)
                return Results.BadRequest("Image File Required.");

            if (!image.ContentType.StartsWith("image/"))
                return Results.BadRequest("Invalid Image File.");

            await using var ms = new MemoryStream();
            await image.CopyToAsync(ms, cancellationToken);

            var request = new ProductSearchRequest
            {
                Page = page,
                PageSize = pageSize
            };

            var result = await productSearchService.SearchByImageAsync(
                ms.ToArray(),
                request,
                cancellationToken);

            return Results.Ok(result);
        })
        .DisableAntiforgery();

        // Create
        productGroup.MapPost("/", async (IProductCommandService productCommandService, CreateProductRequest request, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest("Product Name Required.");
            }

            var result = await productCommandService.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/products/{result.Id}", result);
        });

        // Edit
        productGroup.MapPut("/{id:guid}", async (IProductCommandService productCommandService, Guid id, UpdateProductRequest request, CancellationToken cancellationToken) =>
        {
            request.Id = id;

            if (id == Guid.Empty)
                {
                return Results.BadRequest("Invalid Product Id.");
            }

            if (!await productCommandService.ExistsAsync(id, cancellationToken))
            {
                return Results.NotFound($"Product With Id '{id}' Not Found.");
            }

            var result = await productCommandService.UpdateAsync(request, cancellationToken);
            return Results.Ok(result);
        });

        // Delete
        productGroup.MapDelete("/{id:guid}", async (IProductCommandService productCommandService, Guid id, CancellationToken cancellationToken) =>
        {
            if (id == Guid.Empty)
            {
                return Results.BadRequest("Invalid Product Id.");
            }

            if (!await productCommandService.ExistsAsync(id, cancellationToken))
            {
                return Results.NotFound($"Product With Id '{id}' Not Found.");
            }

            await productCommandService.DeleteAsync(id, cancellationToken);
            return Results.NoContent();
        });

        // Category
        productGroup.MapGet("/categories", () =>
        {
            var categories = new[]
            {
                "Electronic",
                "Elektronik",
                "Fashion",
                "Home",
                "Rumah Tangga",
                "Sports",
                "Olahraga",
                "Stationery",
                "Alat Tulis",
                "Household",
                "Toys",
                "Mainan",
                "Automotive",
                "Otomotif",
                "Books",
                "Buku",
                "Health",
                "Kesehatan",
                "Beauty",
                "Kecantikan",
                "Grocery",
                "Sembako",
                "Garden",
                "Taman",
                "Music",
                "Musik",
                "Office",
                "Perkantoran",
                "Food",
                "Makanan",
                "Drink",
                "Minuman",
                "Pet Supplies",
                "Perlengkapan Hewan",
                "Baby",
                "Perlengkapan Bayi",
                "Industrial",
                "Industri",
                "Art",
                "Seni",
                "Crafts",
                "Kerajinan",
                "Jewelry",
                "Perhiasan",
                "Outdoor",
                "Camping",
                "Travel"
            };

            return Results.Ok(categories.OrderBy(x => x));
        });

    }
}