using LMS.Fee.API.Application.Commands;
using LMS.Fee.API.Domain.Repositories;
using LMS.Fee.API.Infrastructure.Outbox;
using LMS.Fee.API.Infrastructure.Persistence;
using LMS.Fee.API.Infrastructure.Repositories;
using LMS.ServiceDefaults;
using LMS.SharedKernal.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.Services.AddLmsMicroservice<FeeDbContext>(
    builder.Configuration, "LMS Fee API", typeof(RecordFeePaymentCommand).Assembly, builder.Environment);
builder.Services.AddScoped<IFeeInvoiceRepository, FeeInvoiceRepository>();
builder.Services.AddHostedService<FeeOutboxProcessor>();
var app = builder.Build();
await app.EnsureDevelopmentDatabaseAsync<FeeDbContext>();
app.UseLmsDefaults();
app.MapControllers();
app.Run();
