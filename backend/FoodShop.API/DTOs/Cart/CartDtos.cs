namespace FoodShop.API.DTOs.Cart;

public class CartDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(i => i.SubTotal);
    public int TotalItems => Items.Sum(i => i.Quantity);
    public DateTime UpdatedAt { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int AvailableStock { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;
}

public record AddToCartDto(int ProductId, int Quantity);
public record UpdateCartItemDto(int Quantity);
