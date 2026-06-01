using Microsoft.AspNetCore.Builder;
using Serilog;

namespace SchoolErp.Observability.Logging;

public static class SerilogConfig
{
    public static WebApplicationBuilder UseSchoolErpSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
}
