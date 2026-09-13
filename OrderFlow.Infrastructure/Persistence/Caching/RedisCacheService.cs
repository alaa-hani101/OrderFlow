using System.Text.Json;
using StackExchange.Redis;
using OrderFlow.Application.common;

namespace OrderFlow.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();

        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();

        var json = JsonSerializer.Serialize(value);

        await db.StringSetAsync(
            key,
            json,
            expiration);
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();

        await db.KeyDeleteAsync(key);
    }
}