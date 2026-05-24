namespace EMS.Api.Middleware;

using System.Diagnostics;

/// <summary>
/// Performance monitoring middleware
/// Tracks request/response times and logs performance metrics
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var elapsed = stopwatch.ElapsedMilliseconds;
            var request = context.Request;
            var response = context.Response;

            _logger.LogInformation(
                "Request: {Method} {Path} - Status: {StatusCode} - Duration: {ElapsedMs}ms",
                request.Method,
                request.Path,
                response.StatusCode,
                elapsed);

            // Log slow requests (> 1 second)
            if (elapsed > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} - Duration: {ElapsedMs}ms",
                    request.Method,
                    request.Path,
                    elapsed);
            }
        }
    }
}
