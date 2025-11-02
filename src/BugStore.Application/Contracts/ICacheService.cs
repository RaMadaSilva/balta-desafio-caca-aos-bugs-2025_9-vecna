namespace BugStore.Application.Contracts; 

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task PublishInvalidationAsync(string channel, string message, CancellationToken cancellationToken = default);
    Task SubscribeInvalidationAsync(string channel, Action<string> handler, CancellationToken cancellationToken = default);
    Task<long> IncrementVersionAsync(string versionKey, CancellationToken cancellationToken = default);
    Task<long> GetVersionAsync(string versionKey, CancellationToken cancellationToken = default);

}
