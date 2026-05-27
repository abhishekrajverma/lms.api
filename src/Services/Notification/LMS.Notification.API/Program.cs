using LMS.Notification.API.Application.EventHandlers;
using LMS.Notification.API.Infrastructure.Outbox;
using LMS.Notification.API.Infrastructure.Persistence;
using LMS.ServiceDefaults;
using LMS.SharedKernal.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services.AddLmsMicroservice<NotificationDbContext>(
    builder.Configuration, "LMS Notification API", typeof(StudentEnrolledIntegrationEventHandler).Assembly, builder.Environment);
builder.Services.AddHostedService<NotificationOutboxProcessor>();
var app = builder.Build();
await app.EnsureDevelopmentDatabaseAsync<NotificationDbContext>();
app.UseLmsDefaults();
app.MapControllers();
app.Run();
