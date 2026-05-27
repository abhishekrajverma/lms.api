using LMS.ServiceDefaults;
using LMS.SharedKernal.AspNetCore;
using LMS.Student.API.Application.Commands;
using LMS.Student.API.Domain.Repositories;
using LMS.Student.API.Infrastructure.Outbox;
using LMS.Student.API.Infrastructure.Persistence;
using LMS.Student.API.Infrastructure.Repositories;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddLmsMicroservice<StudentDbContext>(
    builder.Configuration, "LMS Student API", typeof(CreateStudentCommand).Assembly, builder.Environment);
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddHostedService<StudentOutboxProcessor>();

var app = builder.Build();
await app.EnsureDevelopmentDatabaseAsync<StudentDbContext>();
app.UseLmsDefaults();
app.MapControllers();
app.Run();

public partial class Program;
