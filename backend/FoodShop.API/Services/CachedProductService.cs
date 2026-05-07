using FoodShop.API.DTOs.Product;
using FoodShop.API.Interfaces;

namespace FoodShop.API.Services;

/// <summary>
/// Decorator pattern: wraps IProductService and adds Redis caching.
/// Registered as the primary IProductService in DI.
/// </summary>
public class CachedProductService : IProductService
{
    private readonly IProductService  _inner;
    private readonly ICacheService    _cache;
    private readonly ILogger<CachedProductService> _logger;

    public CachedProductService(
        ProductService        inner,   // concrete type so DI can resolve without circular ref
        ICacheService         cache,
        ILogger<CachedProductService> logger)
    {
        _inner  = inner;
        _cache  = cache;
        _logger = logger;
    }

    // ── GetProducts ───────────────────────────────────────────────────────────
    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductQueryParams q)
    {
        // Build a deterministic cache key from query params
        var key = $"{RedisCacheService.Keys.Products}:" +
                  $"p{q.Page}:ps{q.PageSize}:cat{q.CategoryId}:" +
                  $"s{q.Search}:sort{q.SortBy}:min{q.MinPrice}:max{q.MaxPrice}";

        var cached = await _cache.GetAsync<PagedResult<ProductDto>>(key);
        if (cached is not null)
        {
            _logger.LogInformation("Returning cached product list. Key: {Key}", key);
            return cached;
        }

        var result = await _inner.GetProductsAsync(q);
        await _cache.SetAsync(key, result, RedisCacheService.ShortExpiry);
        return result;
    }

    // ── GetProductById ────────────────────────────────────────────────────────
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var key = $"{RedisCacheService.Keys.Product}:{id}";

        var cached = await _cache.GetAsync<ProductDto>(key);
        if (cached is not null) return cached;

        var result = await _inner.GetProductByIdAsync(id);
        if (result is not null)
            await _cache.SetAsync(key, result, RedisCacheService.MediumExpiry);

        return result;
    }

    // ── Write operations — invalidate cache ───────────────────────────────────
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var result = await _inner.CreateProductAsync(dto);
        await _cache.RemoveByPrefixAsync(RedisCacheService.Keys.Products);
        _logger.LogInformation("Cache invalidated after product CREATE. Id: {Id}", result.Id);
        return result;
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var result = await _inner.UpdateProductAsync(id, dto);
        if (result is not null)
        {
            await _cache.RemoveAsync($"{RedisCacheService.Keys.Product}:{id}");
            await _cache.RemoveByPrefixAsync(RedisCacheService.Keys.Products);
            _logger.LogInformation("Cache invalidated after product UPDATE. Id: {Id}", id);
        }
        return result;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var result = await _inner.DeleteProductAsync(id);
        if (result)
        {
            await _cache.RemoveAsync($"{RedisCacheService.Keys.Product}:{id}");
            await _cache.RemoveByPrefixAsync(RedisCacheService.Keys.Products);
            _logger.LogInformation("Cache invalidated after product DELETE. Id: {Id}", id);
        }
        return result;
    }
}
