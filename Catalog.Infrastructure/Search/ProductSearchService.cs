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

    public ProductSearchService(HttpClient httpClient, IOptions<ElasticOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<PagedResult<ProductSearchItemDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        var from = (page - 1) * pageSize;

        var must = new List<object>();
        var filter = new List<object>();

        // queryParam search by name
        if (!string.IsNullOrWhiteSpace(request.QueryParam))
        {
            must.Add(new
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

        // jika tidak ada must clause, tambahkan match_all agar query tetap valid
        object mustClause = must.Count == 0
            ? new object[] { new { match_all = new { } } }
            : must;

        // membangun query body untuk Elasticsearch
        var queryBody = new
        {
            from,
            size = pageSize,
            sort,
            query = new
            {
                @bool = new
                {
                    must = mustClause,
                    filter
                }
            }
        };

        // untuk debugging, bisa log queryBody sebelum diserialisasi
        var json = JsonSerializer.Serialize(queryBody);

        // mengirim request ke Elasticsearch
        var response = await _httpClient.PostAsync(
            $"/{_options.IndexName}/_search",
            new StringContent(json, Encoding.UTF8, "application/json"),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        long total = 0;
        var items = new List<ProductSearchItemDto>();

        // parsing response dari Elasticsearch
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

            // parsing setiap hit untuk membangun list ProductSearchItemDto
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

                    if (source.TryGetProperty("id", out var idProp))
                    {
                        if (idProp.ValueKind == JsonValueKind.String)
                        {
                            Guid.TryParse(idProp.GetString(), out id);
                        }
                    }

                    if (source.TryGetProperty("name", out var nameProp))
                    {
                        name = nameProp.GetString() ?? string.Empty;
                    }

                    if (source.TryGetProperty("category", out var categoryProp))
                    {
                        category = categoryProp.GetString() ?? string.Empty;
                    }

                    if (source.TryGetProperty("price", out var priceProp))
                    {
                        if (priceProp.ValueKind == JsonValueKind.Number)
                        {
                            price = priceProp.GetDecimal();
                        }
                    }

                    if (source.TryGetProperty("description", out var descriptionProp))
                    {
                        description = descriptionProp.GetString() ?? string.Empty;
                    }

                    items.Add(new ProductSearchItemDto
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

        // hasil PagedResult
        return new PagedResult<ProductSearchItemDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }
}