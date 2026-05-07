using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodShop.API.Services;

// ── Interface ─────────────────────────────────────────────────────────────────
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}

// ── Redis Implementation ──────────────────────────────────────────────────────
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    // Default cache durations
    public static readonly TimeSpan ShortExpiry  = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan MediumExpiry = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan LongExpiry   = TimeSpan.FromHours(2);

    // Cache key prefixes
    public static class Keys
    {
        public const string Products   = "products";
        public const string Product    = "product";
        public const string Categories = "categories";
        public const string Cart       = "cart";
    }

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache  = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var data = await _cache.GetStringAsync(key);
            if (data is null) return default;

            _logger.LogDebug("Cache HIT: {Key}", key);
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception ex)
        {
            // Cache errors should never crash the app — degrade gracefully
            _logger.LogWarning(ex, "Cache GET failed for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? MediumExpiry
            };
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);
            _logger.LogDebug("Cache SET: {Key} (expires in {Expiry})", key, expiry ?? MediumExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache SET failed for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("Cache REMOVE: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE failed for key: {Key}", key);
        }
    }

    /// <summary>Remove all keys that start with a given prefix (manual pattern match)</summary>
    public async Task RemoveByPrefixAsync(string prefix)
    {
        // Note: IDistributedCache does not support pattern delete natively.
        // For production, inject IConnectionMultiplexer and use SCAN + DEL.
        // Here we remove the most common paginated variants as a practical fallback.
        var keysToRemove = Enumerable.Range(1, 10)
            .SelectMany(page => new[]
            {
                $"{prefix}:p{page}",
                $"{prefix}:p{page}:all",
            })
            .Append(prefix)
            .ToList();

        foreach (var key in keysToRemove)
            await RemoveAsync(key);

        _logger.LogDebug("Cache REMOVE-PREFIX: {Prefix} ({Count} keys)", prefix, keysToRemove.Count);
    }
}
