using EMS.Api.Extensions;
using EMS.Api.Middleware;
using EMS.Infrastructure.Extensions;
using EMS.Infrastructure.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Configure Logging (Serilog)
// ============================================================================
var logger = LoggingConfiguration.ConfigureLogging(builder.Configuration);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// ============================================================================
// Add Services to DI Container
// ============================================================================

// Infrastructure services (repositories, caching, database)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Authentication & Authorization services (JWT, password hashing, etc.)
builder.Services.AddAuthenticationServices(builder.Configuration);

// Application services (AutoMapper, validators, FluentValidation)
builder.Services.AddApiServices();

// ============================================================================
// Configure API Options
// ============================================================================
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                new { success = false, message = "Validation failed", errors });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================================
// Build Application
// ============================================================================
var app = builder.Build();

// ============================================================================
// Configure Middleware Pipeline
// ============================================================================

// Security headers (must be early)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Exception handling (must be early in pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Performance monitoring
app.UseMiddleware<PerformanceMonitoringMiddleware>();

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowSpecific");

// Authentication & Authorization (must be before UseAuthorization)
app.UseAuthentication();
app.UseAuthorization();

// Swagger/OpenAPI documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EMS API V1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
});

// Route mapping
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("Health")
    //.WithOpenApi()
    .AllowAnonymous();

// ============================================================================
// Start Application
// ============================================================================
try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
