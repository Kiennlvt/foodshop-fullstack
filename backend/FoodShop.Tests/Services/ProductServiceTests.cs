using FluentAssertions;
using FoodShop.API.DTOs.Product;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces.Repositories;
using FoodShop.API.Services;
using Moq;

namespace FoodShop.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository>  _productRepoMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _productRepoMock  = new Mock<IProductRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _sut = new ProductService(
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            TestHelpers.CreateMapper());
    }

    // ── GetProducts ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_ReturnsPagedResult()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Fruits" };
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Apple",  Price = 2.99m, Category = category, CategoryId = 1 },
            new() { Id = 2, Name = "Banana", Price = 1.49m, Category = category, CategoryId = 1 },
        };

        _productRepoMock.Setup(r => r.GetPagedAsync(It.IsAny<ProductQueryParams>()))
                        .ReturnsAsync(new PagedResult<Product>
                        {
                            Items      = products,
                            TotalCount = 2,
                            Page       = 1,
                            PageSize   = 10
                        });

        // Act
        var result = await _sut.GetProductsAsync(new ProductQueryParams());

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items[0].Name.Should().Be("Apple");
        result.Items[0].CategoryName.Should().Be("Fruits");
    }

    [Fact]
    public async Task GetProducts_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetPagedAsync(It.IsAny<ProductQueryParams>()))
                        .ReturnsAsync(new PagedResult<Product>
                        {
                            Items = new List<Product>(), TotalCount = 0, Page = 1, PageSize = 10
                        });

        // Act
        var result = await _sut.GetProductsAsync(new ProductQueryParams());

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    // ── GetProductById ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProductById_ExistingId_ReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1, Name = "Apple", Price = 2.99m, StockQuantity = 50,
            Category = new Category { Id = 1, Name = "Fruits" }, CategoryId = 1
        };

        _productRepoMock.Setup(r => r.GetWithCategoryAsync(1))
                        .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Apple");
        result.Price.Should().Be(2.99m);
    }

    [Fact]
    public async Task GetProductById_NotFound_ReturnsNull()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetWithCategoryAsync(999))
                        .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetProductByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ── CreateProduct ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_ValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name          = "Mango",
            Description   = "Sweet mango",
            Price         = 3.99m,
            StockQuantity = 100,
            CategoryId    = 1,
            ImageUrl      = "https://example.com/mango.jpg"
        };

        _categoryRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
                        .ReturnsAsync((Product p) => { p.Id = 10; return p; });

        _productRepoMock.Setup(r => r.GetWithCategoryAsync(10))
                        .ReturnsAsync(new Product
                        {
                            Id = 10, Name = "Mango", Price = 3.99m, CategoryId = 1,
                            Category = new Category { Id = 1, Name = "Fruits" }
                        });

        // Act
        var result = await _sut.CreateProductAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Mango");
        result.Price.Should().Be(3.99m);
    }

    [Fact]
    public async Task CreateProduct_InvalidCategory_ThrowsException()
    {
        // Arrange
        var dto = new CreateProductDto { CategoryId = 999 };

        _categoryRepoMock.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        // Act
        var act = async () => await _sut.CreateProductAsync(dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*Category*not found*");
    }

    // ── DeleteProduct ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteProduct_ExistingId_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Apple", IsActive = true };

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteProductAsync(1);

        // Assert
        result.Should().BeTrue();
        // Verify soft delete — IsActive phải là false
        _productRepoMock.Verify(r => r.UpdateAsync(
            It.Is<Product>(p => p.IsActive == false)), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_NotFound_ReturnsFalse()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.DeleteProductAsync(999);

        // Assert
        result.Should().BeFalse();
    }
}
