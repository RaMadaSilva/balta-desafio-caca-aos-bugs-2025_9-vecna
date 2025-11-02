namespace BugStore.Application.Contracts;

public interface ICacheableRequest
{
    TimeSpan? CacheTtl { get; }
    string CacheRegion { get; }
    bool UseVersioning { get;}
}