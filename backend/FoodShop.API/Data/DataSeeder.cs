using FoodShop.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodShop.API.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Fruits & Vegetables" },
                new() { Name = "Meat & Seafood" },
                new() { Name = "Bakery & Bread" },
                new() { Name = "Dairy & Eggs" },
                new() { Name = "Beverages" },
                new() { Name = "Snacks & Sweets" }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new()
                {
                    Username = "admin",
                    Email = "admin@foodshop.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Username = "john_doe",
                    Email = "john@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        if (!await context.Products.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var fruits = categories.First(c => c.Name == "Fruits & Vegetables");
            var meat   = categories.First(c => c.Name == "Meat & Seafood");
            var bakery = categories.First(c => c.Name == "Bakery & Bread");
            var dairy  = categories.First(c => c.Name == "Dairy & Eggs");
            var bev    = categories.First(c => c.Name == "Beverages");
            var snacks = categories.First(c => c.Name == "Snacks & Sweets");

            var products = new List<Product>
            {
                new() { Name = "Organic Apples",      Description = "Fresh organic apples from local farms",              Price = 3.99m,  StockQuantity = 100, CategoryId = fruits.Id, ImageUrl = "https://images.unsplash.com/photo-1567306226416-28f0efdc88ce?w=400" },
                new() { Name = "Cherry Tomatoes",     Description = "Sweet and juicy cherry tomatoes",                    Price = 2.49m,  StockQuantity = 80,  CategoryId = fruits.Id, ImageUrl = "https://images.unsplash.com/photo-1546094096-0df4bcabd337?w=400" },
                new() { Name = "Fresh Spinach",       Description = "Tender baby spinach leaves, packed with nutrients",  Price = 2.99m,  StockQuantity = 60,  CategoryId = fruits.Id, ImageUrl = "https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=400" },
                new() { Name = "Atlantic Salmon",     Description = "Premium wild-caught Atlantic salmon fillet",         Price = 12.99m, StockQuantity = 40,  CategoryId = meat.Id,   ImageUrl = "https://images.unsplash.com/photo-1580822184713-fc5400e7fe10?w=400" },
                new() { Name = "Chicken Breast",      Description = "Free-range boneless chicken breast",                 Price = 7.49m,  StockQuantity = 55,  CategoryId = meat.Id,   ImageUrl = "https://images.unsplash.com/photo-1604503468506-a8da13d11960?w=400" },
                new() { Name = "Sourdough Bread",     Description = "Artisan sourdough loaf, freshly baked daily",       Price = 5.99m,  StockQuantity = 30,  CategoryId = bakery.Id, ImageUrl = "https://images.unsplash.com/photo-1585478259715-4f9f8cbc8b47?w=400" },
                new() { Name = "Croissants (6-pack)", Description = "Buttery, flaky French-style croissants",            Price = 6.49m,  StockQuantity = 25,  CategoryId = bakery.Id, ImageUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=400" },
                new() { Name = "Whole Milk (1L)",     Description = "Farm-fresh whole milk, rich and creamy",            Price = 1.99m,  StockQuantity = 90,  CategoryId = dairy.Id,  ImageUrl = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=400" },
                new() { Name = "Free-Range Eggs",     Description = "Dozen free-range large eggs",                       Price = 4.29m,  StockQuantity = 70,  CategoryId = dairy.Id,  ImageUrl = "https://images.unsplash.com/photo-1602470521006-4d0cffb1b0b6?w=400" },
                new() { Name = "Greek Yogurt",        Description = "Thick and creamy plain Greek yogurt",               Price = 3.49m,  StockQuantity = 50,  CategoryId = dairy.Id,  ImageUrl = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=400" },
                new() { Name = "Cold Brew Coffee",    Description = "Smooth, low-acidity cold brew coffee concentrate",  Price = 6.99m,  StockQuantity = 45,  CategoryId = bev.Id,    ImageUrl = "https://images.unsplash.com/photo-1517701604599-bb29b565090c?w=400" },
                new() { Name = "Fresh Orange Juice",  Description = "100% squeezed fresh orange juice, no additives",   Price = 4.49m,  StockQuantity = 35,  CategoryId = bev.Id,    ImageUrl = "https://images.unsplash.com/photo-1613478223719-2ab802602423?w=400" },
                new() { Name = "Granola Bar (Box)",   Description = "Wholesome oat and honey granola bars, 12-pack",    Price = 8.99m,  StockQuantity = 60,  CategoryId = snacks.Id, ImageUrl = "https://images.unsplash.com/photo-1590080874088-eec64895b423?w=400" },
                new() { Name = "Dark Chocolate",      Description = "70% cacao premium dark chocolate bar",              Price = 3.99m,  StockQuantity = 80,  CategoryId = snacks.Id, ImageUrl = "https://images.unsplash.com/photo-1606312619070-d48b4c652a52?w=400" },
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
