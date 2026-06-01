using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SchoolErp.BuildingBlocks.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(ILogger<RedisCacheService> logger) => _logger = logger;

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Redis cache get {Key} (no-op stub)", key);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Redis cache set {Key} (no-op stub)", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
