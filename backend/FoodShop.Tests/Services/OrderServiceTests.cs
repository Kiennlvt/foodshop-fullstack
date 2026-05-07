using FluentAssertions;
using FoodShop.API.DTOs.Order;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces.Repositories;
using FoodShop.API.Services;
using Moq;

namespace FoodShop.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository>   _orderRepoMock;
    private readonly Mock<ICartRepository>    _cartRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly OrderService _sut;

    // Dữ liệu dùng chung
    private readonly Product _apple  = new() { Id = 1, Name = "Apple",  Price = 2.99m, StockQuantity = 50 };
    private readonly Product _banana = new() { Id = 2, Name = "Banana", Price = 1.49m, StockQuantity = 30 };

    public OrderServiceTests()
    {
        _orderRepoMock   = new Mock<IOrderRepository>();
        _cartRepoMock    = new Mock<ICartRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _sut = new OrderService(
            _orderRepoMock.Object,
            _cartRepoMock.Object,
            _productRepoMock.Object,
            TestHelpers.CreateMapper());
    }

    // ── PlaceOrder ────────────────────────────────────────────────────────────

    [Fact]
    public async Task PlaceOrder_ValidCart_CreatesOrderAndClearsCart()
    {
        // Arrange
        var cart = new Cart
        {
            Id = 1, UserId = 1,
            CartItems = new List<CartItem>
            {
                new() { ProductId = 1, Quantity = 2, Product = _apple },
                new() { ProductId = 2, Quantity = 3, Product = _banana }
            }
        };

        _cartRepoMock.Setup(r => r.GetOrCreateCartAsync(1)).ReturnsAsync(cart);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_apple);
        _productRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(_banana);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _cartRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Cart>())).Returns(Task.CompletedTask);

        var createdOrder = new Order
        {
            Id = 1, UserId = 1, Status = OrderStatus.Pending,
            TotalPrice = (2.99m * 2) + (1.49m * 3),  // 10.45
            User = new User { Id = 1, Username = "kien" },
            OrderDetails = new List<OrderDetail>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 2.99m, Product = _apple },
                new() { ProductId = 2, Quantity = 3, UnitPrice = 1.49m, Product = _banana }
            }
        };

        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>()))
                      .ReturnsAsync((Order o) => { o.Id = 1; return o; });
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(1))
                      .ReturnsAsync(createdOrder);

        // Act
        var dto = new PlaceOrderDto("123 Main St", "Please deliver after 5pm");
        var result = await _sut.PlaceOrderAsync(1, dto);

        // Assert
        result.Should().NotBeNull();
        result.TotalPrice.Should().Be(10.45m);
        result.Status.Should().Be("Pending");
        result.Items.Should().HaveCount(2);

        // Verify stock bị trừ
        _productRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Product>(p => p.Id == 1 && p.StockQuantity == 48)), Times.Once);
        _productRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Product>(p => p.Id == 2 && p.StockQuantity == 27)), Times.Once);

        // Verify cart bị clear
        _cartRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Cart>(c => c.CartItems.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task PlaceOrder_EmptyCart_ThrowsException()
    {
        // Arrange
        var emptyCart = new Cart { Id = 1, UserId = 1, CartItems = new List<CartItem>() };
        _cartRepoMock.Setup(r => r.GetOrCreateCartAsync(1)).ReturnsAsync(emptyCart);

        // Act
        var act = async () => await _sut.PlaceOrderAsync(1, new PlaceOrderDto(null, null));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*empty cart*");
    }

    [Fact]
    public async Task PlaceOrder_InsufficientStock_ThrowsException()
    {
        // Arrange
        var lowStockProduct = new Product { Id = 1, Name = "Apple", Price = 2.99m, StockQuantity = 1 };
        var cart = new Cart
        {
            Id = 1, UserId = 1,
            CartItems = new List<CartItem>
            {
                new() { ProductId = 1, Quantity = 10, Product = lowStockProduct } // Muốn 10, chỉ có 1
            }
        };

        _cartRepoMock.Setup(r => r.GetOrCreateCartAsync(1)).ReturnsAsync(cart);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(lowStockProduct);

        // Act
        var act = async () => await _sut.PlaceOrderAsync(1, new PlaceOrderDto(null, null));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*Insufficient stock*");
    }

    // ── GetUserOrders ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserOrders_ReturnsOnlyUserOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() {
                Id = 1, UserId = 1, TotalPrice = 10m, Status = OrderStatus.Completed,
                User = new User { Id = 1, Username = "kien" },
                OrderDetails = new List<OrderDetail>()
            }
        };

        _orderRepoMock.Setup(r => r.GetUserOrdersAsync(1)).ReturnsAsync(orders);

        // Act
        var result = await _sut.GetUserOrdersAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(1);
    }

    // ── UpdateOrderStatus ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrderStatus_ValidOrder_UpdatesStatus()
    {
        // Arrange
        var order = new Order
        {
            Id = 1, UserId = 1, Status = OrderStatus.Pending,
            User = new User { Id = 1, Username = "kien" },
            OrderDetails = new List<OrderDetail>()
        };

        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(1)).ReturnsAsync(order);
        _orderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateOrderStatusAsync(1, new UpdateOrderStatusDto(OrderStatus.Completed));

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task UpdateOrderStatus_OrderNotFound_ReturnsNull()
    {
        // Arrange
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(999)).ReturnsAsync((Order?)null);

        // Act
        var result = await _sut.UpdateOrderStatusAsync(999, new UpdateOrderStatusDto(OrderStatus.Completed));

        // Assert
        result.Should().BeNull();
    }
}
