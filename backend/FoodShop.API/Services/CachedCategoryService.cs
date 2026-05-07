using FoodShop.API.DTOs.Category;
using FoodShop.API.Interfaces;

namespace FoodShop.API.Services;

/// <summary>
/// Decorator: adds Redis caching to ICategoryService.
/// Categories change rarely — use a long TTL.
/// </summary>
public class CachedCategoryService : ICategoryService
{
    private readonly ICategoryService _inner;
    private readonly ICacheService    _cache;
    private readonly ILogger<CachedCategoryService> _logger;

    private const string AllCategoriesKey = RedisCacheService.Keys.Categories;

    public CachedCategoryService(
        CategoryService       inner,
        ICacheService         cache,
        ILogger<CachedCategoryService> logger)
    {
        _inner  = inner;
        _cache  = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var cached = await _cache.GetAsync<List<CategoryDto>>(AllCategoriesKey);
        if (cached is not null)
        {
            _logger.LogInformation("Returning cached categories list.");
            return cached;
        }

        var result = (await _inner.GetCategoriesAsync()).ToList();
        await _cache.SetAsync(AllCategoriesKey, result, RedisCacheService.LongExpiry);
        return result;
    }

    public Task<CategoryDto?> GetCategoryByIdAsync(int id) =>
        _inner.GetCategoryByIdAsync(id);

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var result = await _inner.CreateCategoryAsync(dto);
        await _cache.RemoveAsync(AllCategoriesKey);
        return result;
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var result = await _inner.UpdateCategoryAsync(id, dto);
        await _cache.RemoveAsync(AllCategoriesKey);
        return result;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var result = await _inner.DeleteCategoryAsync(id);
        if (result) await _cache.RemoveAsync(AllCategoriesKey);
        return result;
    }
}
