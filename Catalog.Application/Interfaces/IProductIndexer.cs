namespace Catalog.Application.Interfaces;

public interface IProductIndexer
{
    Task ReindexAllAsync(CancellationToken cancellationToken = default);
}