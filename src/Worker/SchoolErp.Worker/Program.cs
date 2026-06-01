using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolErp.BuildingBlocks.Infrastructure.EventBus;
using SchoolErp.BuildingBlocks.Infrastructure.Outbox;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.EventBus;
using SchoolErp.Modules.TenantManagement.Persistence;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<TenantManagementDbContext>(options =>
    options.ConfigureSchoolErpDatabase(builder.Configuration, "TenantManagement"));

if (builder.Environment.IsDevelopment())
    builder.Services.AddSingleton<IRabbitPublisher, NoOpRabbitPublisher>();
else
    builder.Services.AddSchoolErpEventBus(builder.Configuration);

builder.Services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<TenantManagementDbContext>());
builder.Services.AddHostedService<OutboxProcessorWorker>();
builder.Services.AddHostedService<OutboxDispatchWorker>();

var host = builder.Build();
host.Run();
