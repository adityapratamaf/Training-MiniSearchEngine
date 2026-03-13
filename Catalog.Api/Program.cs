using Catalog.Api.Endpoints;
using Catalog.Application.Interfaces;
using Catalog.Infrastructure.DependencyInjection;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ECommerce.Api",
        Version = "v1"
    });
});

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

// api documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ECommerce.Api");
        options.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
    });
}

app.UseCors("AllowFrontend");

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

    // TODO : untuk seeding data awal, sebaiknya dilakukan hanya sekali saja, jika sudah seeding maka bagian ini bisa dikomentari.
    // var seeder = services.GetRequiredService<IProductSeeder>();
    // await seeder.SeedAsync();

    // TODO : untuk reindex produk ke elasticsearch, sebaiknya dilakukan hanya sekali saja setelah seeding data, jika sudah reindex maka bagian ini bisa dikomentari.
    //var indexer = services.GetRequiredService<IProductIndexer>();
    //await indexer.ReindexAllAsync();
}

app.Run();