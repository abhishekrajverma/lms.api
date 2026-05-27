using LMS.SharedKernal.Primitives;
using Microsoft.AspNetCore.Http;

namespace LMS.SharedKernal.AspNetCore;

/// <summary>
/// Resolves tenant from X-Tenant-Id header or tenant_id JWT claim.
/// </summary>
public sealed class HttpTenantContext : ITenantContext
{
    public const string TenantHeaderName = "X-Tenant-Id";
    public const string TenantClaimType = "tenant_id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId => ResolveTenantId();

    public string TenantIdentifier => TenantId.ToString();

    public bool IsResolved => TenantId != Guid.Empty;

    private Guid ResolveTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return Guid.Empty;

        if (httpContext.Request.Headers.TryGetValue(TenantHeaderName, out var headerValue)
            && Guid.TryParse(headerValue.FirstOrDefault(), out var headerTenantId))
        {
            return headerTenantId;
        }

        var claim = httpContext.User.FindFirst(TenantClaimType)?.Value
                    ?? httpContext.User.FindFirst("tid")?.Value;

        return Guid.TryParse(claim, out var claimTenantId) ? claimTenantId : Guid.Empty;
    }
}
