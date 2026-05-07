using FoodShop.API.Entities;

namespace FoodShop.API.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public List<OrderDetailDto> Items { get; set; } = new();
}

public class OrderDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;
}

public record PlaceOrderDto(
    string? ShippingAddress,
    string? Notes
);

public record UpdateOrderStatusDto(OrderStatus Status);
