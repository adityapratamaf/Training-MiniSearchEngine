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
    /// Mengirim semua data product dari PostgreSQL ke Elasticsearch secara bertahap (batch),
    /// agar request bulk tidak terlalu besar dan tidak memicu error 413 Payload Too Large.
    /// </summary>
    public async Task ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        await EnsureIndexAsync(cancellationToken);

        const int batchSize = 1000;

        var total = await _dbContext.Products.CountAsync(cancellationToken);

        for (var offset = 0; offset < total; offset += batchSize)
        {
            var products = await _dbContext.Products
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip(offset)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (products.Count == 0)
                break;

            var sb = new StringBuilder();

            foreach (var p in products)
            {
                var meta = new
                {
                    index = new
                    {
                        _index = _options.ProductsIndex,
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
            var response = await _httpClient.PostAsync("/_bulk", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            Console.WriteLine($"Indexed {Math.Min(offset + batchSize, total)} / {total} products");
        }

        await _httpClient.PostAsync($"/{_options.ProductsIndex}/_refresh", null, cancellationToken);
    }

    /// <summary>
    /// Memastikan index produk sudah ada di Elasticsearch.
    /// Jika belum ada, maka index akan dibuat beserta mapping field-nya.
    /// </summary>
    private async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        var exists = await _httpClient.GetAsync($"/{_options.ProductsIndex}", cancellationToken);

        if (exists.IsSuccessStatusCode) return;

        var mapping = """
        {
          "mappings": {
            "properties": {
              "id":           { "type": "keyword" },
              "name":         { "type": "search_as_you_type" },
              "category":     { "type": "keyword" },
              "price":        { "type": "double" },
              "description":  { "type": "text" },
              "createdAtUtc": { "type": "date" }
            }
          }
        }
        """;

        var content = new StringContent(mapping, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/{_options.ProductsIndex}", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

