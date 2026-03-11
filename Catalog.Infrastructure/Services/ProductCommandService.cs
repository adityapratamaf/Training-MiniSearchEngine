using Catalog.Application.Contracts;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Services;

// <summary>
// service untuk menangani operasi create, update, delete pada produk,
// sekaligus memastikan bahwa setiap perubahan produk akan mengirimkan pesan ke antrian untuk diindeks ulang di Elasticsearch.
// </summary>
public class ProductCommandService : IProductCommandService
{
    private readonly AppDbContext _dbContext;
    private readonly IProductIndexQueue _productIndexQueue;

    public ProductCommandService(AppDbContext dbContext, IProductIndexQueue productIndexQueue)
    {
        _dbContext = dbContext;
        _productIndexQueue = productIndexQueue;
    }

    public async Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Category = request.Category,
            Price = request.Price,
            Description = request.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // setelah data produk berhasil dibuat (create), kirim pesan ke antrian untuk diindeks ulang di Elasticsearch
        await _productIndexQueue.EnqueueAsync(new ProductIndexMessage
        {
            ProductId = product.Id,
            Action = "Upsert"
        }, cancellationToken);

        return product;
    }

    public async Task<Product> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product is null)
            throw new KeyNotFoundException($"Product with id '{request.Id}' was not found.");

        product.Name = request.Name;
        product.Category = request.Category;
        product.Price = request.Price;
        product.Description = request.Description;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // setelah data produk berhasil diperbarui (update), kirim pesan ke antrian untuk diindeks ulang di Elasticsearch
        await _productIndexQueue.EnqueueAsync(new ProductIndexMessage
        {
            ProductId = product.Id,
            Action = "Upsert"
        }, cancellationToken);

        return product;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null)
            throw new KeyNotFoundException($"Product with id '{id}' was not found.");

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // setelah data produk berhasil dihapus (delete), kirim pesan ke antrian untuk diindeks ulang di Elasticsearch
        await _productIndexQueue.EnqueueAsync(new ProductIndexMessage
        {
            ProductId = id,
            Action = "Delete"
        }, cancellationToken);
    }
}