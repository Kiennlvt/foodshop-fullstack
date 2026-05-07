using FoodShop.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodShop.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Role).HasDefaultValue("User");
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.HasIndex(p => p.Name);               // For search
            e.HasIndex(p => p.CategoryId);         // For filtering
            e.HasIndex(p => p.IsActive);           // For active-only queries
            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Cart
        modelBuilder.Entity<Cart>(e =>
        {
            e.HasIndex(c => c.UserId).IsUnique();  // One cart per user
            e.HasOne(c => c.User)
             .WithOne(u => u.Cart)
             .HasForeignKey<Cart>(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem
        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
            e.HasOne(ci => ci.Cart)
             .WithMany(c => c.CartItems)
             .HasForeignKey(ci => ci.CartId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ci => ci.Product)
             .WithMany(p => p.CartItems)
             .HasForeignKey(ci => ci.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.TotalPrice).HasPrecision(18, 2);
            e.HasIndex(o => o.UserId);
            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.OrderDate);
            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderDetail
        modelBuilder.Entity<OrderDetail>(e =>
        {
            e.Property(od => od.UnitPrice).HasPrecision(18, 2);
            e.HasOne(od => od.Order)
             .WithMany(o => o.OrderDetails)
             .HasForeignKey(od => od.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(od => od.Product)
             .WithMany(p => p.OrderDetails)
             .HasForeignKey(od => od.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
