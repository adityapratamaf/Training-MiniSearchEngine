namespace Catalog.Infrastructure.Search;

/// <summary>
/// untuk menyimpan konfigurasi Elasticsearch seperti URL dasar dan nama indeks.
/// </summary>
public class ElasticOptions
{
    public const string SectionName = "Elastic";

    public string BaseUrl { get; set; } = string.Empty;

    public string ProductsIndex { get; set; } = string.Empty;
    //public string CategoriesIndex { get; set; } = string.Empty;
    //public string BrandsIndex { get; set; } = string.Empty;
}