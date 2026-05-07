using FluentAssertions;
using FoodShop.API.DTOs.Category;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces.Repositories;
using FoodShop.API.Services;
using Moq;

namespace FoodShop.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _repoMock = new Mock<ICategoryRepository>();
        _sut = new CategoryService(_repoMock.Object, TestHelpers.CreateMapper());
    }

    [Fact]
    public async Task GetCategories_ReturnsMappedList()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fruits", Products = new List<Product> { new(), new() } },
            new() { Id = 2, Name = "Bakery", Products = new List<Product>() }
        };
        _repoMock.Setup(r => r.GetAllWithCountAsync()).ReturnsAsync(categories);

        // Act
        var result = (await _sut.GetCategoriesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Fruits");
        result[0].ProductCount.Should().Be(2);
        result[1].ProductCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateCategory_DuplicateName_ThrowsException()
    {
        // Arrange
        _repoMock.Setup(r => r.NameExistsAsync("Fruits")).ReturnsAsync(true);

        // Act
        var act = async () => await _sut.CreateCategoryAsync(new CreateCategoryDto("Fruits"));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateCategory_ValidName_ReturnsCreated()
    {
        // Arrange
        _repoMock.Setup(r => r.NameExistsAsync("Dairy")).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
                 .ReturnsAsync((Category c) => { c.Id = 5; return c; });

        // Act
        var result = await _sut.CreateCategoryAsync(new CreateCategoryDto("Dairy"));

        // Assert
        result.Name.Should().Be("Dairy");
        result.Id.Should().Be(5);
    }

    [Fact]
    public async Task DeleteCategory_NotFound_ReturnsFalse()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Category?)null);

        // Act
        var result = await _sut.DeleteCategoryAsync(999);

        // Assert
        result.Should().BeFalse();
    }
}
