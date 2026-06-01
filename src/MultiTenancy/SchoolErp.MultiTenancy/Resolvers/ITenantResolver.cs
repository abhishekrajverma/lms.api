using Microsoft.AspNetCore.Http;

namespace SchoolErp.MultiTenancy.Resolvers;

public interface ITenantResolver
{
    Task<Guid?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}

public sealed class HeaderTenantResolver : ITenantResolver
{
    public const string HeaderName = "X-Tenant-Id";

    public Task<Guid?> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        if (httpContext.Request.Headers.TryGetValue(HeaderName, out var value) &&
            Guid.TryParse(value.FirstOrDefault(), out var tenantId))
            return Task.FromResult<Guid?>(tenantId);

        return Task.FromResult<Guid?>(null);
    }
}
