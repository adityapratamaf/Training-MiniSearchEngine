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

        var random = new Random();
        var products = new List<Catalog.Domain.Entities.Product>();

        for (var i = 0; i < 1000; i++)
        {
            products.Add(ProductFactory.Create(random));
        }

        await _dbContext.Products.AddRangeAsync(products, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}