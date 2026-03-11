using Catalog.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.BackgroundJobs;

// <summary>
// background service untuk memproses antrian indexing produk ke Elasticsearch secara asinkron
// </summary>
public class ProductIndexBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IProductIndexQueue _productIndexQueue;
    private readonly ILogger<ProductIndexBackgroundService> _logger;

    public ProductIndexBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IProductIndexQueue productIndexQueue,
        ILogger<ProductIndexBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _productIndexQueue = productIndexQueue;
        _logger = logger;
    }

    // method utama dari background service yang akan terus berjalan selama aplikasi berjalan,
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProductIndexBackgroundService started...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _productIndexQueue.DequeueAsync(stoppingToken);

                using var scope = _serviceScopeFactory.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IElasticProductSyncService>();

                switch (message.Action)
                {
                    case "Upsert":
                        await syncService.UpsertAsync(message.ProductId, stoppingToken);
                        _logger.LogInformation("Product {ProductId} upserted to Elasticsearch.", message.ProductId);
                        break;

                    case "Delete":
                        await syncService.DeleteAsync(message.ProductId, stoppingToken);
                        _logger.LogInformation("Product {ProductId} deleted from Elasticsearch.", message.ProductId);
                        break;

                    default:
                        _logger.LogWarning("Unknown product index action: {Action}", message.Action);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing product index background job.");
            }
        }

        _logger.LogInformation("ProductIndexBackgroundService stopped...");
    }
}