using Catalog.Application.Interfaces;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Seeding;

public class ProductSeeder : IProductSeeder
{
    private readonly AppDbContext _dbContext;

    public ProductSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var count = await _dbContext.Products.CountAsync(cancellationToken);
        if (count > 0) return;

        const int totalData = 500000;
        const int batchSize = 1000;

        var random = new Random();

        for (var i = 0; i < totalData; i += batchSize)
        {
            var products = new List<Catalog.Domain.Entities.Product>(batchSize);

            for (var j = 0; j < batchSize && i + j < totalData; j++)
            {
                products.Add(ProductFactory.Create(random));
            }

            await _dbContext.Products.AddRangeAsync(products, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _dbContext.ChangeTracker.Clear();

            Console.WriteLine($"Seeded {Math.Min(i + batchSize, totalData)} / {totalData}");
        }
    }
}
