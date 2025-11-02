using BugStore.Application.Contracts;
using BugStore.Application.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BugStore.Application.Behavior;

public class CacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableRequest
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheBehavior<TRequest, TResponse>> _logger;

    public CacheBehavior(ICacheService cacheService, ILogger<CacheBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var version = request.UseVersioning
            ? await _cacheService.GetVersionAsync(request.CacheRegion, cancellationToken)
            : 1;

        var key = $"{request.CacheRegion}:v{version}:{KeyRole.HashFilters(request)}";

        var cached = await _cacheService.GetAsync<TResponse>(key, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit: {Key}", key);
            return cached;
        }

        _logger.LogInformation("Cache miss: {Key}", key);
        var response = await next(); 

        await _cacheService.SetAsync(key, response, request.CacheTtl, cancellationToken);
        return response;
    }
}
