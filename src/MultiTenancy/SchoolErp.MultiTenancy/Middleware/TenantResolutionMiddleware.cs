using Microsoft.AspNetCore.Http;
using SchoolErp.MultiTenancy.Resolvers;

namespace SchoolErp.MultiTenancy.Middleware;

public sealed class TenantResolutionMiddleware
{
    private static readonly string[] AnonymousPrefixes =
    [
        "/health",
        "/swagger",
        "/api/v1/tenants"
    ];

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantResolver resolver, ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (IsAnonymous(path, context.Request.Method))
        {
            await _next(context);
            return;
        }

        var tenantId = await resolver.ResolveAsync(context, context.RequestAborted);
        if (!tenantId.HasValue)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Tenant is required. Provide X-Tenant-Id header.");
            return;
        }

        tenantContext.SetTenant(tenantId.Value);
        await _next(context);
    }

    private static bool IsAnonymous(string path, string method)
    {
        if (AnonymousPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return true;
        if (path.Equals("/api/v1/tenants", StringComparison.OrdinalIgnoreCase) &&
            method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }
}
