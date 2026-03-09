/// <summary>
/// untuk menyimpan konfigurasi Elasticsearch seperti URL dasar dan nama indeks.
/// </summary>
namespace Catalog.Infrastructure.Search;

public class ElasticOptions
{
    public string BaseUrl { get; set; } = "https://elastic.minimdev.com";
    public string IndexName { get; set; } = "products";
}