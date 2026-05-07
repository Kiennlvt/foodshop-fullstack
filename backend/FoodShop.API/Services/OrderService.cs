using AutoMapper;
using FoodShop.API.DTOs.Order;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces;
using FoodShop.API.Interfaces.Repositories;

namespace FoodShop.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository   _orderRepo;
    private readonly ICartRepository    _cartRepo;
    private readonly IProductRepository _productRepo;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository   orderRepo,
        ICartRepository    cartRepo,
        IProductRepository productRepo,
        IMapper            mapper)
    {
        _orderRepo   = orderRepo;
        _cartRepo    = cartRepo;
        _productRepo = productRepo;
        _mapper      = mapper;
    }

    public async Task<OrderDto> PlaceOrderAsync(int userId, PlaceOrderDto dto)
    {
        var cart = await _cartRepo.GetOrCreateCartAsync(userId);

        if (!cart.CartItems.Any())
            throw new InvalidOperationException("Cannot place an order with an empty cart.");

        // Validate stock & snapshot prices
        var details = new List<OrderDetail>();
        decimal total = 0;

        foreach (var item in cart.CartItems)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId)
                ?? throw new KeyNotFoundException($"Product {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}");

            details.Add(new OrderDetail
            {
                ProductId = item.ProductId,
                Quantity  = item.Quantity,
                UnitPrice = product.Price
            });

            // Deduct stock
            product.StockQuantity -= item.Quantity;
            await _productRepo.UpdateAsync(product);

            total += product.Price * item.Quantity;
        }

        var order = new Order
        {
            UserId          = userId,
            TotalPrice      = total,
            Status          = OrderStatus.Pending,
            ShippingAddress = dto.ShippingAddress,
            Notes           = dto.Notes,
            OrderDetails    = details
        };

        await _orderRepo.AddAsync(order);

        // Clear cart after successful order
        cart.CartItems.Clear();
        await _cartRepo.UpdateAsync(cart);

        var created = await _orderRepo.GetOrderWithDetailsAsync(order.Id);
        return _mapper.Map<OrderDto>(created!);
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
    {
        var orders = await _orderRepo.GetUserOrdersAsync(userId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId, string role)
    {
        var order = await _orderRepo.GetOrderWithDetailsAsync(orderId);
        if (order is null) return null;

        // Users can only see their own orders; Admins can see all
        if (role != "Admin" && order.UserId != userId) return null;

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepo.GetAllOrdersAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
    {
        var order = await _orderRepo.GetOrderWithDetailsAsync(orderId);
        if (order is null) return null;

        order.Status = dto.Status;
        await _orderRepo.UpdateAsync(order);

        return _mapper.Map<OrderDto>(order);
    }
}
