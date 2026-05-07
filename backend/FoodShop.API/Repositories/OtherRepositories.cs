using FoodShop.API.Data;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodShop.API.Repositories;

// ── Category Repository ──────────────────────────────────────────────────────
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Category>> GetAllWithCountAsync()
        => await _context.Categories
            .Include(c => c.Products.Where(p => p.IsActive))
            .ToListAsync();

    public async Task<bool> NameExistsAsync(string name)
        => await _context.Categories.AnyAsync(c => c.Name == name);
}

// ── Cart Repository ──────────────────────────────────────────────────────────
public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context) => _context = context;

    public async Task<Cart?> GetCartByUserIdAsync(int userId)
        => await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart != null) return cart;

        cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        // Re-load with navigation
        return (await GetCartByUserIdAsync(userId))!;
    }

    public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        => await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);

    public async Task UpdateAsync(Cart cart)
    {
        cart.UpdatedAt = DateTime.UtcNow;
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();
    }
}

// ── Order Repository ─────────────────────────────────────────────────────────
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        => await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        => await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        => await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
}
