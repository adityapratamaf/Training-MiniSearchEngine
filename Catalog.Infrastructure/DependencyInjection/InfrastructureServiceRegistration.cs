using Catalog.Application.Interfaces;
using Catalog.Infrastructure.BackgroundJobs;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Search;
using Catalog.Infrastructure.Seeding;
using Catalog.Infrastructure.Services;
using Catalog.Infrastructure.Ocr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    // <summary>
    // untuk menambahkan service ke dalam container dependency injection
    // </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        // untuk register dan validasi konfigurasi elasticoptions ke DI container
        services
            .AddOptions<ElasticOptions>()
            .Bind(configuration.GetSection(ElasticOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "Elastic:BaseUrl wajib diisi.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ProductsIndex), "Elastic:ProductsIndex wajib diisi.")
            .ValidateOnStart();

        services.Configure<ElasticOptions>(configuration.GetSection("Elastic"));

        // mendaftarkan productindexer service dengan BaseAddress Elasticsearch dari konfigurasi
        services.AddHttpClient<IProductIndexer, ProductIndexer>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        // mendaftarkan HttpClient service untuk ProductSearchService dengan BaseAddress Elasticsearch dari konfigurasi
        services.AddHttpClient<IProductSearchService, ProductSearchService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        // ocr service untuk mengekstrak teks dari gambar menggunakan Tesseract OCR
        services.AddScoped<IOcrService, TesseractOcrService>();

        // mendaftarkan sync service untuk menyinkronkan data produk dari database ke Elasticsearch
        services.AddScoped<IProductCommandService, ProductCommandService>();
        services.AddSingleton<IProductIndexQueue, InMemoryProductIndexQueue>();
        services.AddHostedService<ProductIndexBackgroundService>();

        services.AddHttpClient<IElasticProductSyncService, ElasticProductSyncService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        // seeding data
        services.AddScoped<IProductSeeder, ProductSeeder>();

        return services;
    }
}