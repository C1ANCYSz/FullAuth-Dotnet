using StackExchange.Redis;

namespace AuthApp.Features.Redis;

public sealed class RedisService(IConnectionMultiplexer redis)
{
    private readonly IDatabase _db = redis.GetDatabase();

    public Task SetAsync(string key, string value)
    {
        return _db.StringSetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public Task RemoveAsync(string key)
    {
        return _db.KeyDeleteAsync(key);
    }
}
