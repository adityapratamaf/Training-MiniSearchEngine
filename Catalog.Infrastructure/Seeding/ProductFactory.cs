using Catalog.Domain.Entities;

namespace Catalog.Infrastructure.Seeding;

public static class ProductFactory
{
    private static readonly string[] Categories =
    [
        "Electronics",
        "Fashion",
        "Home",
        "Sports",
        "Stationery",
        "Household",
        "Toys",
        "Automotive",
        "Books",
        "Health",
        "Beauty",
        "Grocery",
        "Garden",
        "Music",
        "Office",
        "Food",
        "Drink"

    ];

    private static readonly string[] Adjectives =
    [
        "Premium", "Smart", "Portable", "Gaming", "Modern", "Wireless", "Waterproof", "High-Quality", "Eco-Friendly", "Multi-functional",
        "Elegant", "Compact", "Pro", "Lite", "Ultra", "Classic", "Advanced", "Durable", "Stylish"
    ];

    private static readonly string[] ProductNames =
    [
        "Headphone", "Laptop", "Mouse", "Keyboard", "Monitor", "Tablet", "Cholate", "Perfume", "Syrup", "Juice", "Camera", "Cup", "Bottle", "Bag", 
        "Phone", "Shoes", "Chair", "Table", "Watch", "Backpack", "Camera", "Sunglasses", "Jacket", "Book", "Bicycle", "Blender", "Coffee Maker", "Vacuum Cleaner", "Smart TV"
    ];

    public static Product Create(Random random)
    {
        var category = Categories[random.Next(Categories.Length)];
        var adjective = Adjectives[random.Next(Adjectives.Length)];
        var name = ProductNames[random.Next(ProductNames.Length)];

        var price = Math.Round((decimal)(random.NextDouble() * 9_500_000 + 500_000), 2);

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = $"{adjective} {name}",
            Category = category,
            Price = price,
            Description = $"This is a {adjective.ToLower()} {name.ToLower()} for {category.ToLower()} category.",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-random.Next(0, 365))
        };
    }
}