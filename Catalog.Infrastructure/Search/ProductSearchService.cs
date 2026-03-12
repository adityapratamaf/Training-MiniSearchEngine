using System.Text;
using System.Text.Json;
using Catalog.Application.Contracts;
using Catalog.Application.Dtos;
using Catalog.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure.Search;

public class ProductSearchService : IProductSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ElasticOptions _options;
    private readonly IOcrService _ocrService;

    public ProductSearchService(HttpClient httpClient, IOptions<ElasticOptions> options, IOcrService ocrService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _ocrService = ocrService;
    }

    // <summary>
    // service untuk melakukan pencarian dengan input berupa gambar yang menggunakan OCR untuk mengekstrak teks dari gambar, lalu menggunakan teks tersebut sebagai query pencarian di Elasticsearch.
    // </summary>
    public async Task<PagedResponse<ProductResponse>> SearchByImageAsync(byte[] imageBytes, ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        var extractedText = await _ocrService.ExtractTextAsync(imageBytes, cancellationToken);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return new PagedResponse<ProductResponse>
            {
                Page = request.Page <= 0 ? 1 : request.Page,
                PageSize = request.PageSize <= 0 ? 10 : request.PageSize,
                Total = 0,
                Items = new List<ProductResponse>()
            };
        }

        request.QueryParam = extractedText;

        return await SearchAsync(request, cancellationToken);
    }

    /// <summary>
    /// service untuk melakukan pencarian product menggunakan elasticsearch
    /// </summary>
    public async Task<PagedResponse<ProductResponse>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        var from = (page - 1) * pageSize;

        var must = new List<object>();
        var filter = new List<object>();
        var should = new List<object>();

        // queryParam search by name
        if (!string.IsNullOrWhiteSpace(request.QueryParam))
        {
            // autocomplete / prefix search
            should.Add(new
            {
                multi_match = new
                {
                    query = request.QueryParam,
                    type = "bool_prefix",
                    fields = new[]
                    {
                        "name",
                        "name._2gram",
                        "name._3gram"
                    },
                    boost = 2
                }
            });

            // fuzzy search untuk typo tolerance
            should.Add(new
            {
                multi_match = new
                {
                    query = request.QueryParam,
                    fields = new[]
                    {
                        "name^3",
                        "description",
                        "category"
                    },
                    fuzziness = "AUTO",
                    prefix_length = 1,
                    @operator = "and"
                }
            });
        }

        // get by category filtering
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            filter.Add(new
            {
                term = new
                {
                    category = request.Category
                }
            });
        }

        // get by range price filtering
        if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
        {
            filter.Add(new
            {
                range = new
                {
                    price = new
                    {
                        gte = request.MinPrice,
                        lte = request.MaxPrice
                    }
                }
            });
        }

        // helper to lower sortBy
        object[] sort = request.SortPrice?.ToLower() switch
        {
            "asc" => new object[] { new { price = new { order = "asc" } } },
            "desc" => new object[] { new { price = new { order = "desc" } } },
            _ => new object[] { new { _score = new { order = "desc" } } }
        };

        // jika tidak ada query search, pakai match_all
        object queryClause = should.Count == 0
            ? new
            {
                @bool = new
                {
                    must = new object[] { new { match_all = new { } } },
                    filter
                }
            }
            : new
            {
                @bool = new
                {
                    should,
                    minimum_should_match = 1,
                    filter
                }
            };

        // membangun query body untuk elasticsearch
        var queryBody = new
        {
            from,
            size = pageSize,
            sort,
            query = queryClause
        };

        var json = JsonSerializer.Serialize(queryBody);

        // mengirim request ke elasticsearch
        var response = await _httpClient.PostAsync(
            $"/{_options.ProductsIndex}/_search",
            new StringContent(json, Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        long total = 0;
        var items = new List<ProductResponse>();

        // parsing response dari elasticsearch
        if (root.TryGetProperty("hits", out var hitsElement))
        {
            if (hitsElement.TryGetProperty("total", out var totalElement))
            {
                if (totalElement.ValueKind == JsonValueKind.Object &&
                    totalElement.TryGetProperty("value", out var totalValue))
                {
                    total = totalValue.GetInt64();
                }
                else if (totalElement.ValueKind == JsonValueKind.Number)
                {
                    total = totalElement.GetInt64();
                }
            }

            if (hitsElement.TryGetProperty("hits", out var hitArray) &&
                hitArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var hit in hitArray.EnumerateArray())
                {
                    if (!hit.TryGetProperty("_source", out var source))
                        continue;

                    Guid id = Guid.Empty;
                    string name = string.Empty;
                    string category = string.Empty;
                    decimal price = 0;
                    string description = string.Empty;

                    if (source.TryGetProperty("id", out var idProp) &&
                        idProp.ValueKind == JsonValueKind.String)
                    {
                        Guid.TryParse(idProp.GetString(), out id);
                    }

                    if (source.TryGetProperty("name", out var nameProp))
                    {
                        name = nameProp.GetString() ?? string.Empty;
                    }

                    if (source.TryGetProperty("category", out var categoryProp))
                    {
                        category = categoryProp.GetString() ?? string.Empty;
                    }

                    if (source.TryGetProperty("price", out var priceProp) &&
                        priceProp.ValueKind == JsonValueKind.Number)
                    {
                        price = priceProp.GetDecimal();
                    }

                    if (source.TryGetProperty("description", out var descriptionProp))
                    {
                        description = descriptionProp.GetString() ?? string.Empty;
                    }

                    items.Add(new ProductResponse
                    {
                        Id = id,
                        Name = name,
                        Category = category,
                        Price = price,
                        Description = description
                    });
                }
            }
        }

        return new PagedResponse<ProductResponse>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }
}