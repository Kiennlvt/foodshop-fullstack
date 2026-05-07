using FoodShop.API.DTOs.Category;
using FoodShop.API.DTOs.Product;
using FoodShop.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodShop.API.Controllers;

// ── Products Controller ───────────────────────────────────────────────────────
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    /// <summary>Get paginated list of products with filtering and search</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] ProductQueryParams queryParams)
    {
        var result = await _service.GetProductsAsync(queryParams);
        return Ok(result);
    }

    /// <summary>Get a single product by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _service.GetProductByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Create a new product (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var product = await _service.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>Update an existing product (Admin only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _service.UpdateProductAsync(id, dto);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Soft-delete a product (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var success = await _service.DeleteProductAsync(id);
        return success ? NoContent() : NotFound();
    }
}

// ── Categories Controller ─────────────────────────────────────────────────────
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        => Ok(await _service.GetCategoriesAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var cat = await _service.GetCategoryByIdAsync(id);
        return cat is null ? NotFound() : Ok(cat);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var cat = await _service.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { id = cat.Id }, cat);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var cat = await _service.UpdateCategoryAsync(id, dto);
        return cat is null ? NotFound() : Ok(cat);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var success = await _service.DeleteCategoryAsync(id);
        return success ? NoContent() : NotFound();
    }
}
