using LMS.Academic.API.Application.Commands;
using LMS.Academic.API.Domain.Repositories;
using LMS.Academic.API.Infrastructure.Outbox;
using LMS.Academic.API.Infrastructure.Persistence;
using LMS.Academic.API.Infrastructure.Repositories;
using LMS.ServiceDefaults;
using LMS.SharedKernal.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services.AddLmsMicroservice<AcademicDbContext>(
    builder.Configuration, "LMS Academic API", typeof(CreateClassCommand).Assembly, builder.Environment);
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddHostedService<AcademicOutboxProcessor>();
var app = builder.Build();
await app.EnsureDevelopmentDatabaseAsync<AcademicDbContext>();
app.UseLmsDefaults();
app.MapControllers();
app.Run();
