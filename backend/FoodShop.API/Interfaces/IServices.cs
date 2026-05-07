using FoodShop.API.DTOs.Auth;
using FoodShop.API.DTOs.Cart;
using FoodShop.API.DTOs.Category;
using FoodShop.API.DTOs.Order;
using FoodShop.API.DTOs.Product;
using FoodShop.API.Entities;

namespace FoodShop.API.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductQueryParams queryParams);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddToCartAsync(int userId, AddToCartDto dto);
    Task<CartDto> UpdateCartItemAsync(int userId, int productId, UpdateCartItemDto dto);
    Task<CartDto> RemoveCartItemAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
}

public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(int userId, PlaceOrderDto dto);
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId, string role);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);
}
