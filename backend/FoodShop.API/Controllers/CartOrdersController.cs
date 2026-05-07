using FoodShop.API.DTOs.Cart;
using FoodShop.API.DTOs.Order;
using FoodShop.API.Helpers;
using FoodShop.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodShop.API.Controllers;

// ── Cart Controller ───────────────────────────────────────────────────────────
[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _service;

    public CartController(ICartService service) => _service = service;

    /// <summary>Get the current user's cart</summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var cart = await _service.GetCartAsync(User.GetUserId());
        return Ok(cart);
    }

    /// <summary>Add a product to the cart</summary>
    [HttpPost("add")]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartDto dto)
    {
        var cart = await _service.AddToCartAsync(User.GetUserId(), dto);
        return Ok(cart);
    }

    /// <summary>Update item quantity</summary>
    [HttpPut("items/{productId:int}")]
    public async Task<ActionResult<CartDto>> UpdateItem(int productId, [FromBody] UpdateCartItemDto dto)
    {
        var cart = await _service.UpdateCartItemAsync(User.GetUserId(), productId, dto);
        return Ok(cart);
    }

    /// <summary>Remove an item from the cart</summary>
    [HttpDelete("items/{productId:int}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int productId)
    {
        var cart = await _service.RemoveCartItemAsync(User.GetUserId(), productId);
        return Ok(cart);
    }

    /// <summary>Clear all items from the cart</summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        await _service.ClearCartAsync(User.GetUserId());
        return NoContent();
    }
}

// ── Orders Controller ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    /// <summary>Place an order from the current cart</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] PlaceOrderDto dto)
    {
        var order = await _service.PlaceOrderAsync(User.GetUserId(), dto);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>Get current user's order history</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        var orders = await _service.GetUserOrdersAsync(User.GetUserId());
        return Ok(orders);
    }

    /// <summary>Get a specific order by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _service.GetOrderByIdAsync(id, User.GetUserId(), User.GetUserRole());
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Get all orders (Admin only)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        var orders = await _service.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>Update order status (Admin only)</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _service.UpdateOrderStatusAsync(id, dto);
        return order is null ? NotFound() : Ok(order);
    }
}
