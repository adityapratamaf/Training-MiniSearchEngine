namespace Catalog.Infrastructure.Search;

public class ElasticOptions
{
    public string BaseUrl { get; set; } = "https://elastic.minimdev.com";
    public string IndexName { get; set; } = "products";
}