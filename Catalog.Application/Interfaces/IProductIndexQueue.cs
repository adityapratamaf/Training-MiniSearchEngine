using Catalog.Application.Contracts;

namespace Catalog.Application.Interfaces;

public interface IProductIndexQueue
{
    ValueTask EnqueueAsync(ProductIndexMessage message, CancellationToken cancellationToken = default);
    ValueTask<ProductIndexMessage> DequeueAsync(CancellationToken cancellationToken);
}