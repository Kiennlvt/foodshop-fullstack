namespace FoodShop.API.Entities;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    // Navigation
    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
