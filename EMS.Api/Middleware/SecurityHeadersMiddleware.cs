namespace EMS.Api.Middleware;

/// <summary>
/// Security headers middleware
/// Adds security-related HTTP headers to all responses
/// Helps prevent common web vulnerabilities (XSS, CSRF, Clickjacking, etc.)
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        AddSecurityHeaders(context.Response);
        
        await _next(context);
    }

    private void AddSecurityHeaders(HttpResponse response)
    {
        _logger.LogDebug("Adding security headers to response");

        // Prevent MIME type sniffing
        // Forces browser to respect Content-Type header
        response.Headers.Add("X-Content-Type-Options", "nosniff");

        // Prevents the page from being displayed inside an iframe on another website
        // Protects against clickjacking attacks
        response.Headers.Add("X-Frame-Options", "DENY");

        // Legacy XSS protection header (modern browsers use CSP)
        // Blocks page if XSS attack is detected
        response.Headers.Add("X-XSS-Protection", "1; mode=block");

        // Content Security Policy
        // Restricts where resources can be loaded from
        response.Headers.Add("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'");

        // HSTS - Forces HTTPS
        // Tells browser to always use HTTPS for this domain
        response.Headers.Add("Strict-Transport-Security",
            "max-age=31536000; includeSubDomains; preload");

        // Referrer Policy
        // Controls what referrer information is sent
        response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions Policy (formerly Feature Policy)
        // Controls which features can be used in the browser
        response.Headers.Add("Permissions-Policy",
            "geolocation=(), " +
            "microphone=(), " +
            "camera=(), " +
            "payment=(), " +
            "usb=(), " +
            "magnetometer=(), " +
            "gyroscope=(), " +
            "accelerometer=()");

        // Disable caching for sensitive responses
        if (ShouldDisableCache(response.StatusCode))
        {
            response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Expires", "0");
        }

        _logger.LogDebug("Security headers added successfully");
    }

    private bool ShouldDisableCache(int statusCode)
    {
        // Disable cache for sensitive status codes
        return statusCode switch
        {
            401 => true, // Unauthorized
            403 => true, // Forbidden
            404 => false, // Not Found - can cache
            500 => false, // Server Error - don't cache
            _ => false
        };
    }
}
