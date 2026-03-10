using Catalog.Application.Interfaces;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Search;
using Catalog.Infrastructure.Seeding;
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

        // mendaftarkan productindexer dengan BaseAddress Elasticsearch dari konfigurasi
        services.AddHttpClient<IProductIndexer, ProductIndexer>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddHttpClient<IProductSearchService, ProductSearchService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        // seeding data
        services.AddScoped<IProductSeeder, ProductSeeder>();

        return services;
    }
}