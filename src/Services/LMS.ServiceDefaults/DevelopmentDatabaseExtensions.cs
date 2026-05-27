using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LMS.ServiceDefaults;

public static class DevelopmentDatabaseExtensions
{
    public static async Task EnsureDevelopmentDatabaseAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        if (!app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
