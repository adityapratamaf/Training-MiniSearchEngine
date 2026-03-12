namespace Catalog.Application.Interfaces;

public interface IOcrService
{
    Task<string> ExtractTextAsync(byte[] fileBytes, CancellationToken cancellationToken = default);
}