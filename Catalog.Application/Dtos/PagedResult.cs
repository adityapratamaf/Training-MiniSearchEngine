namespace Catalog.Application.Dtos;

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
}