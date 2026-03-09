namespace Catalog.Application.Interfaces;

public interface IProductSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}