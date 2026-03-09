using Catalog.Api.Endpoints;
using Catalog.Application.Interfaces;
using Catalog.Infrastructure.DependencyInjection;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

// cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "http://127.0.0.1:5501",
                "http://localhost:5501"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapProductEndpoints();

// <summary>
// untuk memastikan bahwa database sudah dibuat dan migrasi sudah diterapkan,
// lalu melakukan seeding data awal ke database, dan akhirnya melakukan reindex semua produk ke elasticsearch.
// </summary>
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seeder = services.GetRequiredService<IProductSeeder>();
    await seeder.SeedAsync();

    var indexer = services.GetRequiredService<IProductIndexer>();
    await indexer.ReindexAllAsync();
}

app.Run();