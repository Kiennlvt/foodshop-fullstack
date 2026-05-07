using FoodShop.API.DTOs.Product;
using FoodShop.API.Entities;

namespace FoodShop.API.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
}

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}

public interface IProductRepository : IGenericRepository<Product>
{
    Task<PagedResult<Product>> GetPagedAsync(ProductQueryParams queryParams);
    Task<Product?> GetWithCategoryAsync(int id);
}

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IEnumerable<Category>> GetAllWithCountAsync();
    Task<bool> NameExistsAsync(string name);
}

public interface ICartRepository
{
    Task<Cart?> GetCartByUserIdAsync(int userId);
    Task<Cart> GetOrCreateCartAsync(int userId);
    Task<CartItem?> GetCartItemAsync(int cartId, int productId);
    Task UpdateAsync(Cart cart);
}

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
    Task<Order?> GetOrderWithDetailsAsync(int orderId);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
}
