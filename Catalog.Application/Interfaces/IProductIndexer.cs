// <summary>
// interface untuk mengindeks produk ke Elasticsearch. Ini memiliki satu metode, ReindexAllAsync, yang akan mengambil semua produk dari database dan mengirimkannya ke Elasticsearch dalam format bulk.
// </summary>
namespace Catalog.Application.Interfaces;

public interface IProductIndexer
{
    Task ReindexAllAsync(CancellationToken cancellationToken = default);
}