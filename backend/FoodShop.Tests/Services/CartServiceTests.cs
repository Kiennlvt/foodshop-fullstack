using FluentAssertions;
using FoodShop.API.DTOs.Cart;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces.Repositories;
using FoodShop.API.Services;
using Moq;

namespace FoodShop.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository>    _cartRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly CartService _sut;

    // Dữ liệu dùng chung
    private readonly Product _sampleProduct = new()
    {
        Id = 1, Name = "Apple", Price = 2.99m, StockQuantity = 50,
        ImageUrl = "https://example.com/apple.jpg"
    };

    private readonly Cart _sampleCart = new()
    {
        Id = 1, UserId = 1, CartItems = new List<CartItem>()
    };

    public CartServiceTests()
    {
        _cartRepoMock    = new Mock<ICartRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _sut = new CartService(
            _cartRepoMock.Object,
            _productRepoMock.Object,
            TestHelpers.CreateMapper());
    }

    // ── GetCart ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCart_ReturnsCartForUser()
    {
        // Arrange
        _cartRepoMock.Setup(r => r.GetOrCreateCartAsync(1))
                     .ReturnsAsync(_sampleCart);

        // Act
        var result = await _sut.GetCartAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(1);
        result.Items.Should().BeEmpty();
    }

    // ── AddToCart ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddToCart_NewItem_AddsSuccessfully()
    {
        // Arrange
        var dto = new AddToCartDto(ProductId: 1, Quantity: 2);

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_sampleProduct);
        _cartRepoMock.Setup(r => r.GetOrCreateCartAsync(1)).ReturnsAsync(_sampleCart);
        _cartRepoMock.Setup(r => r.GetCartItemAsync(1, 1)).ReturnsAsync((CartItem?)null);
        _cartRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Cart>())).Returns(Task.CompletedTask);

        // Cart sau khi update
        var updatedCart = new Cart
        {
            Id = 1, UserId = 1,
            CartItems = new List<CartItem>
            {
                new() { Id = 1, ProductId = 1, Quantity = 2, Product = _sampleProduct }
            }
        };
        _cartRepoMock.SetupSequence(r => r.GetOrCreateCartAsync(1))
                     .ReturnsAsync(_sampleCart)
                     .ReturnsAsync(updatedCart);

        // Act
        var result = await _sut.AddToCartAsync(1, dto);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].ProductName.Should().Be("Apple");
    }

    [Fact]
    public async Task AddToCart_ProductNotFound_ThrowsException()
    {
        // Arrange
        var dto = new AddToCartDto(ProductId: 999, Quantity: 1);
        _productRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var act = async () => await _sut.AddToCartAsync(1, dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*Product not found*");
    }

    [Fact]
    public async Task AddToCart_ExceedsStock_ThrowsException()
    {
        // Arrange
        var dto = new AddToCartDto(ProductId: 1, Quantity: 100); // Vượt quá stock 50
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_sampleProduct);
        _cartRepoMock.Setup(r => r.GetOrCreateCartAsync(1)).ReturnsAsync(_sampleCart);
        _cartRepoMock.Setup(r => r.GetCartItemAsync(1, 1)).ReturnsAsync((CartItem?)null);

        // Act
        var act = async () => await _sut.AddToCartAsync(1, dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*stock*");
    }

    // ── RemoveCartItem ────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveCartItem_ExistingItem_RemovesSuccessfully()
    {
        // Arrange
        var cartItem = new CartItem { Id = 1, ProductId = 1, CartId = 1, Product = _sampleProduct };
        var cart = new Cart { Id = 1, UserId = 1, CartItems = new List<CartItem> { cartItem } };

        _cartRepoMock.Setup(r => r.GetOrCreateCartAsync(1)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetCartItemAsync(1, 1)).ReturnsAsync(cartItem);
        _cartRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Cart>())).Returns(Task.CompletedTask);

        var emptyCart = new Cart { Id = 1, UserId = 1, CartItems = new List<CartItem>() };
        _cartRepoMock.SetupSequence(r => r.GetOrCreateCartAsync(1))
                     .ReturnsAsync(cart)
                     .ReturnsAsync(emptyCart);

        // Act
        var result = await _sut.RemoveCartItemAsync(1, 1);

        // Assert
        result.Items.Should().BeEmpty();
        _cartRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Cart>()), Times.Once);
    }

    [Fact]
    public async Task RemoveCartItem_NotFound_ThrowsException()
    {
        // Arrange
        _cartRepoMock.Setup(r => r.GetOrCreateCartAsync(1)).ReturnsAsync(_sampleCart);
        _cartRepoMock.Setup(r => r.GetCartItemAsync(1, 999)).ReturnsAsync((CartItem?)null);

        // Act
        var act = async () => await _sut.RemoveCartItemAsync(1, 999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*Cart item not found*");
    }
}
