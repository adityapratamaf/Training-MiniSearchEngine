using Catalog.Domain.Entities;
using System.Xml.Linq;

namespace Catalog.Infrastructure.Seeding;

public static class ProductFactory
{
    private static readonly string[] Categories =
    [
        "Electronic",
        "Elektronik",
        "Fashion",
        "Home",
        "Rumah Tangga",
        "Sports",
        "Olahraga",
        "Stationery",
        "Alat Tulis",
        "Household",
        "Toys",
        "Mainan",
        "Automotive",
        "Otomotif",
        "Books",
        "Buku",
        "Health",
        "Kesehatan",
        "Beauty",
        "Kecantikan",
        "Grocery",
        "Sembako",
        "Garden",
        "Taman",
        "Music",
        "Musik",
        "Office",
        "Perkantoran",
        "Food",
        "Makanan",
        "Drink",
        "Minuman",
        "Pet Supplies",
        "Perlengkapan Hewan",
        "Baby",
        "Perlengkapan Bayi",
        "Industrial",
        "Industri",
        "Art",
        "Seni",
        "Crafts",
        "Kerajinan",
        "Jewelry",
        "Perhiasan",
        "Outdoor",
        "Camping",
        "Travel"
    ];

    private static readonly string[] Adjectives =
    [
        "Premium",
        "Smart",
        "Portable",
        "Gaming",
        "Modern",
        "Wireless",
        "Waterproof",
        "High Quality",
        "Eco-Friendly",
        "Multi-functional",
        "Elegant",
        "Compact",
        "Pro",
        "Lite",
        "Ultra",
        "Classic",
        "Advanced",
        "Durable",
        "Stylish",
        "Big",
        "Small",
        "Super",
        "Medium",
        "Low",
        "Original",
        "Limited",
        "Exclusive",
        "Best Seller",
        "Hemat",
        "Murah",
        "Ekonomis",
        "Praktis",
        "Kuat",
        "Anti Air",
        "Serbaguna",
        "Minimalis",
        "Kekinian",
        "Premium Quality"
    ];

    private static readonly string[] ProductNames =
    [
        "Headphone",
        "Laptop",
        "Mouse",
        "Keyboard",
        "Monitor",
        "Tablet",
        "Chocolate",
        "Perfume",
        "Syrup",
        "Juice",
        "Camera",
        "Cup",
        "Bottle",
        "Bag",
        "Cable",
        "T-Shirt",
        "Adapter",
        "Tangga",
        "Phone",
        "Shoes",
        "Chair",
        "Table",
        "Watch",
        "Backpack",
        "Sunglasses",
        "Jacket",
        "Book",
        "Bicycle",
        "Blender",
        "Coffee Maker",
        "Vacuum Cleaner",
        "Smart TV",
        "Charger",
        "Meja",
        "Kursi",
        "Lampu",
        "Rice Cooker",
        "Dispenser",
        "Kompor",
        "Helmet",
        "Power Bank",
        "Speaker",
        "Headset",
        "Tas",
        "Dompet",
        "Sepatu",
        "Sendal",
        "Botol Minum",
        "Notebook",
        "Pulpen",
        "Pensil",
        "Kamera DSLR",
        "Tripod",
        "Flashdisk",
        "Harddisk",
        "Router WiFi",
        "Sumatera",
        "Sumatra",
        "North Sumatera",
        "Sumatra Barat",
        "Bakpia Yogyakarta",
        "Jogjakarta Bakpia"
    ];

    public static Product Create(Random random)
    {
        var category = Categories[random.Next(Categories.Length)];
        var adjective = Adjectives[random.Next(Adjectives.Length)];
        var name = ProductNames[random.Next(ProductNames.Length)];
        var price = (decimal)random.Next(10_000, 5_000_001);

        var descriptions = new[]
        {
            $"This {adjective.ToLower()} {name.ToLower()} is perfect for the {category.ToLower()} category.",
            $"Produk {name.ToLower()} dengan kualitas {adjective.ToLower()}, cocok untuk kategori {category.ToLower()}.",
            $"A {adjective.ToLower()} {name.ToLower()} designed for everyday use in the {category.ToLower()} category.",
            $"{name} ini memiliki kualitas {adjective.ToLower()} dan sangat cocok untuk kebutuhan {category.ToLower()}.",
            $"Experience the {adjective.ToLower()} {name.ToLower()} made for the {category.ToLower()} category.",
            $"{adjective} {name} yang dirancang khusus untuk kategori {category.ToLower()}.",
            $"Upgrade your {category.ToLower()} needs with this {adjective.ToLower()} {name.ToLower()}.",
            $"{name} berkualitas {adjective.ToLower()} yang cocok digunakan untuk kebutuhan {category.ToLower()} sehari-hari."
        };

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = $"{name} {adjective}",
            Category = category,
            Price = price,
            Description = descriptions[random.Next(descriptions.Length)];
            CreatedAtUtc = DateTime.UtcNow.AddDays(-random.Next(0, 365))
        };
    }
}