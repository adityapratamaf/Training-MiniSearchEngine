// <summary>
// untuk mengisi database dengan data produk awal. Ini akan membuat beberapa produk contoh dan menyimpannya ke database jika belum ada data produk.
// </summary>
namespace Catalog.Application.Interfaces;

public interface IProductSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}