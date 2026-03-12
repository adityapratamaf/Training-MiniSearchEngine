using Catalog.Application.Contracts;
using Catalog.Domain.Entities;

namespace Catalog.Application.Interfaces;

public interface IProductCommandService
{
    Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Product> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}