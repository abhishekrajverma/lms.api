using LMS.SharedKernal.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LMS.SharedKernal.AspNetCore;

/// <summary>
/// Rejects requests that do not carry a valid tenant identifier (except health/swagger paths).
/// </summary>
public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

  private static readonly PathString[] _excludedPaths =
    [
        new("/health"),
        new("/swagger"),
        new("/alive")
    ];

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (_excludedPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            await _next(context);
            return;
        }

        if (!tenantContext.IsResolved)
        {
            _logger.LogWarning("Request rejected: tenant not resolved for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Tenant is required. Provide X-Tenant-Id header or tenant_id claim."
            });
            return;
        }

        await _next(context);
    }
}
