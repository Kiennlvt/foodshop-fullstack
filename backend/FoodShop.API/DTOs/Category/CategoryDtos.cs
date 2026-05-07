namespace FoodShop.API.DTOs.Category;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public record CreateCategoryDto(string Name);
public record UpdateCategoryDto(string Name);
