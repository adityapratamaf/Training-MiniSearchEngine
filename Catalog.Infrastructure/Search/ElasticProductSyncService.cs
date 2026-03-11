using System.Net;
using System.Text;
using System.Text.Json;
using Catalog.Application.Interfaces;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure.Search;

// <summary>
// service untuk menyinkronkan data produk ke Elasticsearch,
// digunakan oleh ProductIndexBackgroundService untuk memproses antrian indexing produk secara asinkron
// </summary>
public class ElasticProductSyncService : IElasticProductSyncService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;
    private readonly ElasticOptions _options;

    public ElasticProductSyncService(HttpClient httpClient, AppDbContext dbContext, IOptions<ElasticOptions> options)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _options = options.Value;
    }

    // metode untuk menambahkan atau memperbarui data produk di Elasticsearch
    public async Task UpsertAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        await EnsureIndexAsync(cancellationToken);

        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

        if (product is null)
            return;

        var doc = new
        {
            id = product.Id,
            name = product.Name,
            category = product.Category,
            price = product.Price,
            description = product.Description,
            createdAtUtc = product.CreatedAtUtc
        };

        var json = JsonSerializer.Serialize(doc);

        // gunakan HTTP PUT untuk upsert (update jika sudah ada, atau create jika belum ada) dokumen produk di Elasticsearch
        var response = await _httpClient.PutAsync(
            $"/{_options.ProductsIndex}/_doc/{product.Id}",
            new StringContent(json, Encoding.UTF8, "application/json"),
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to upsert product '{product.Id}' to Elasticsearch. Status: {(int)response.StatusCode} {response.StatusCode}. Response: {responseBody}");
        }
    }

    // metode untuk menghapus data produk dari Elasticsearch
    public async Task DeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        await EnsureIndexAsync(cancellationToken);

        // gunakan HTTP DELETE untuk menghapus dokumen produk dari
        var response = await _httpClient.DeleteAsync(
            $"/{_options.ProductsIndex}/_doc/{productId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        // jika status code bukan 200 OK atau 404 Not Found, maka anggap sebagai kegagalan dan lempar exception
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to delete product '{productId}' from Elasticsearch. Status: {(int)response.StatusCode} {response.StatusCode}. Response: {responseBody}");
        }
    }

    // metode untuk memastikan bahwa index produk sudah ada di Elasticsearch, jika belum maka akan dibuat dengan mapping yang sesuai
    private async Task EnsureIndexAsync(CancellationToken cancellationToken)
    {
        var exists = await _httpClient.GetAsync($"/{_options.ProductsIndex}", cancellationToken);

        if (exists.IsSuccessStatusCode)
            return;

        if (exists.StatusCode != HttpStatusCode.NotFound)
        {
            var existsBody = await exists.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception(
                $"Failed to check index '{_options.ProductsIndex}'. Status: {(int)exists.StatusCode} {exists.StatusCode}. Response: {existsBody}");
        }

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

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to create index '{_options.ProductsIndex}'. Status: {(int)response.StatusCode} {response.StatusCode}. Response: {responseBody}");
        }
    }
}

