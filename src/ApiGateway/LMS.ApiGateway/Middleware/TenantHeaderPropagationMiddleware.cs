using LMS.SharedKernal.AspNetCore;

namespace LMS.ApiGateway.Middleware;

/// <summary>
/// Ensures X-Tenant-Id from JWT claim is forwarded to downstream microservices.
/// </summary>
public sealed class TenantHeaderPropagationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantHeaderPropagationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(HttpTenantContext.TenantHeaderName))
        {
            var tenantClaim = context.User.FindFirst(HttpTenantContext.TenantClaimType)?.Value
                              ?? context.User.FindFirst("tid")?.Value;

            if (Guid.TryParse(tenantClaim, out _))
                context.Request.Headers[HttpTenantContext.TenantHeaderName] = tenantClaim;
        }

        await _next(context);
    }
}
