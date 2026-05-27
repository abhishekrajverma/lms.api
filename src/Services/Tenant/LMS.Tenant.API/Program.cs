using LMS.ServiceDefaults;
using LMS.SharedKernal.AspNetCore;
using LMS.Tenant.API.Application.Commands;
using LMS.Tenant.API.Infrastructure.Outbox;
using LMS.Tenant.API.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddLmsRegistryMicroservice<TenantDbContext>(
    builder.Configuration,
    "LMS Tenant API",
    typeof(RegisterTenantCommand).Assembly,
    builder.Environment);

builder.Services.AddScoped<LMS.Tenant.API.Domain.Repositories.ISchoolTenantRepository,
    LMS.Tenant.API.Infrastructure.Repositories.SchoolTenantRepository>();
builder.Services.AddHostedService<TenantOutboxProcessor>();

var app = builder.Build();
await app.EnsureDevelopmentDatabaseAsync<TenantDbContext>();
app.UseLmsDefaults(requireTenant: false);
app.MapControllers();
app.Run();
