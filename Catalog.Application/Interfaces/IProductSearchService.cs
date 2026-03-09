using Catalog.Application.Contracts;
using Catalog.Application.Dtos;

namespace Catalog.Application.Interfaces;

public interface IProductSearchService
{
    Task<PagedResult<ProductSearchItemDto>> SearchAsync( ProductSearchRequest request, CancellationToken cancellationToken = default);
}