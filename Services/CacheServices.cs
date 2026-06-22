using System.Text.Json;
using StackExchange.Redis;
using TraineeManagementApi.RedisCaching.ServiceInterface;

namespace TraineeManagementApi.RedisCaching.Service;

public class CacheService : ICacheService
{
    private readonly IDatabase _db;

    public CacheService(IConnectionMultiplexer connection)
    {
        _db = connection.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        RedisValue value = await _db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        string json = JsonSerializer.Serialize(value);
        
        await _db.StringSetAsync(key, json, ttl);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}

