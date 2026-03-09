namespace Catalog.Application.Contracts;

// <summary>
// untuk menyimpan parameter pencarian produk yang akan digunakan untuk membangun query pencarian di elasticsearch.
// </summary>
public class ProductSearchRequest
{
    public string? QueryParam { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortPrice { get; set; } 
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}