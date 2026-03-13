using Catalog.Application.Contracts;
using Catalog.Application.Dtos;

namespace Catalog.Application.Interfaces;

// <summary>
// untuk melakukan pencarian produk berdasarkan kata kunci, kategori, dan rentang harga.
// Ini akan membangun query pencarian yang sesuai untuk Elasticsearch, mengirimkannya, dan kemudian memetakan hasilnya ke DTO yang akan dikembalikan ke klien.
// </summary>
public interface IProductSearchService
{
    Task<PagedResponse<ProductResponse>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken = default);

    Task<PagedResponse<ProductResponse>> SearchByImageAsync(byte[] imageBytes, ProductSearchRequest request, CancellationToken cancellationToken = default);
}