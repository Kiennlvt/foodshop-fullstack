using AutoMapper;
using FoodShop.API.DTOs.Cart;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces;
using FoodShop.API.Interfaces.Repositories;

namespace FoodShop.API.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepo;
    private readonly IProductRepository _productRepo;
    private readonly IMapper _mapper;

    public CartService(ICartRepository cartRepo, IProductRepository productRepo, IMapper mapper)
    {
        _cartRepo    = cartRepo;
        _productRepo = productRepo;
        _mapper      = mapper;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _cartRepo.GetOrCreateCartAsync(userId);
        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto> AddToCartAsync(int userId, AddToCartDto dto)
    {
        var product = await _productRepo.GetByIdAsync(dto.ProductId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (product.StockQuantity < dto.Quantity)
            throw new InvalidOperationException($"Only {product.StockQuantity} items in stock.");

        var cart = await _cartRepo.GetOrCreateCartAsync(userId);
        var existing = await _cartRepo.GetCartItemAsync(cart.Id, dto.ProductId);

        if (existing is not null)
        {
            var newQty = existing.Quantity + dto.Quantity;
            if (newQty > product.StockQuantity)
                throw new InvalidOperationException($"Only {product.StockQuantity} items in stock.");

            existing.Quantity = newQty;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                CartId    = cart.Id,
                ProductId = dto.ProductId,
                Quantity  = dto.Quantity
            });
        }

        await _cartRepo.UpdateAsync(cart);
        var updated = await _cartRepo.GetOrCreateCartAsync(userId);
        return _mapper.Map<CartDto>(updated);
    }

    public async Task<CartDto> UpdateCartItemAsync(int userId, int productId, UpdateCartItemDto dto)
    {
        if (dto.Quantity <= 0)
            return await RemoveCartItemAsync(userId, productId);

        var cart = await _cartRepo.GetOrCreateCartAsync(userId);
        var item = await _cartRepo.GetCartItemAsync(cart.Id, productId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        var product = await _productRepo.GetByIdAsync(productId)!;
        if (product!.StockQuantity < dto.Quantity)
            throw new InvalidOperationException($"Only {product.StockQuantity} items in stock.");

        item.Quantity = dto.Quantity;
        await _cartRepo.UpdateAsync(cart);

        var updated = await _cartRepo.GetOrCreateCartAsync(userId);
        return _mapper.Map<CartDto>(updated);
    }

    public async Task<CartDto> RemoveCartItemAsync(int userId, int productId)
    {
        var cart = await _cartRepo.GetOrCreateCartAsync(userId);
        var item = await _cartRepo.GetCartItemAsync(cart.Id, productId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        cart.CartItems.Remove(item);
        await _cartRepo.UpdateAsync(cart);

        var updated = await _cartRepo.GetOrCreateCartAsync(userId);
        return _mapper.Map<CartDto>(updated);
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _cartRepo.GetOrCreateCartAsync(userId);
        cart.CartItems.Clear();
        await _cartRepo.UpdateAsync(cart);
    }
}
