using BugStore.Application.Contracts;
using StackExchange.Redis;
using System.Text.Json;

namespace BugStore.Infrastructure.Caching;

public class RedisCacheService : ICacheService, IDisposable
{
    private readonly IConnectionMultiplexer _conn;
    private readonly IDatabase _db; 
    private readonly ISubscriber _sub;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _instancePrefix;

    public RedisCacheService(IConnectionMultiplexer connection, string instancePrefix = "bugstore",
        JsonSerializerOptions? jsonOptions = null)
    {
        _conn = connection;
        _db = _conn.GetDatabase();
        _sub = _conn.GetSubscriber();
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _instancePrefix = instancePrefix;
    }

    private string Prefixed(string key) => $"{_instancePrefix}:{key}";

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var v = await _db.StringGetAsync(Prefixed(key));

        if (v.IsNullOrEmpty) 
            return default;

        return JsonSerializer.Deserialize<T>(v, _jsonOptions);
    }

    public async Task<long> GetVersionAsync(string versionKey, CancellationToken cancellationToken = default)
    {
        var v = await _db.StringGetAsync(Prefixed($"version:{versionKey}"));
        if (v.IsNullOrEmpty) return 0;
        return (long)v;
    }

    public Task<long> IncrementVersionAsync(string versionKey, CancellationToken cancellationToken = default)
           => _db.StringIncrementAsync(Prefixed($"version:{versionKey}"));

    public Task PublishInvalidationAsync(string channel, string message, CancellationToken cancellationToken = default)
          => _sub.PublishAsync(Prefixed(channel), message);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => _db.KeyDeleteAsync(Prefixed(key));

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);
        await _db.StringSetAsync(Prefixed(key), bytes, ttl);
    }

    public async Task SubscribeInvalidationAsync(string channel, Action<string> handler, CancellationToken cancellationToken = default)
    {
        await _sub.SubscribeAsync(Prefixed(channel), (redisChannel, value) =>
        {
            handler(value);
        });
    }

    public void Dispose()
    {
        _conn?.Dispose();
    }
}
