namespace Catalog.Application.Interfaces;

public interface IElasticProductSyncService
{
    Task UpsertAsync(Guid productId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid productId, CancellationToken cancellationToken = default);
}