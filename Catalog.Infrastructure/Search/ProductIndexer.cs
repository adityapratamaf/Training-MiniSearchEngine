using System.Text;
using System.Text.Json;
using Catalog.Application.Interfaces;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure.Search;

public class ProductIndexer : IProductIndexer
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;
    private readonly ElasticOptions _options;

    public ProductIndexer(HttpClient httpClient, AppDbContext dbContext, IOptions<ElasticOptions> options)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _options = options.Value;
    }

    /// <summary>
    /// mengirim semua data product dari db postgre ke elasticsearch
    /// </summary>
    public async Task ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        await EnsureIndexAsync(cancellationToken);

        var products = await _dbContext.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();

        foreach (var p in products)
        {
            var meta = new
            {
                index = new
                {
                    _index = _options.IndexName,
                    _id = p.Id
                }
            };

            var doc = new
            {
                id = p.Id,
                name = p.Name,
                category = p.Category,
                price = p.Price,
                description = p.Description,
                createdAtUtc = p.CreatedAtUtc
            };

            sb.AppendLine(JsonSerializer.Serialize(meta));
            sb.AppendLine(JsonSerializer.Serialize(doc));
        }

        var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/x-ndjson");
        var response = await _httpClient.PostAsync("/_bulk?refresh=true", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    // <summary>
    // set indeks untuk produk sudah ada di elasticsearch
    // </summary>
    private async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        var exists = await _httpClient.GetAsync($"/{_options.IndexName}", cancellationToken);

        if (exists.IsSuccessStatusCode) return;

        var mapping = """
        {
          "mappings": {
            "properties": {
              "id":          { "type": "keyword" },
              "name":        { "type": "search_as_you_type" },
              "category":    { "type": "keyword" },
              "price":       { "type": "double" },
              "description": { "type": "text" },
              "createdAtUtc":{ "type": "date" }
            }
          }
        }
        """;

        var content = new StringContent(mapping, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/{_options.IndexName}", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}