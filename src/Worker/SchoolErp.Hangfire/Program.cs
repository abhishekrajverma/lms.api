using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolErp.Hangfire.Jobs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHangfire(c => c.UseMemoryStorage());
builder.Services.AddHangfireServer();
builder.Services.AddHostedService<HangfireJobScheduler>();

var app = builder.Build();
app.UseHangfireDashboard();
app.Run();
