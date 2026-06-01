using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Application;
using SchoolErp.BuildingBlocks.Infrastructure.Caching;
using SchoolErp.BuildingBlocks.Infrastructure.EventBus;
using SchoolErp.EventBus;
using SchoolErp.Modules.Academics.Api;
using SchoolErp.Modules.Academics.Application.Commands;
using SchoolErp.Modules.Academics.Persistence;
using SchoolErp.Modules.Attendance.Api;
using SchoolErp.Modules.Attendance.Application.Commands;
using SchoolErp.Modules.Attendance.Persistence;
using SchoolErp.Modules.Audit.Api;
using SchoolErp.Modules.Audit.Application.Commands;
using SchoolErp.Modules.Audit.Persistence;
using SchoolErp.Modules.Billing.Api;
using SchoolErp.Modules.Billing.Application.Commands;
using SchoolErp.Modules.Billing.Persistence;
using SchoolErp.Modules.Communication.Api;
using SchoolErp.Modules.Communication.Application.Commands;
using SchoolErp.Modules.Communication.Persistence;
using SchoolErp.Modules.Employees.Api;
using SchoolErp.Modules.Employees.Application.Commands;
using SchoolErp.Modules.Employees.Persistence;
using SchoolErp.Modules.Examinations.Api;
using SchoolErp.Modules.Examinations.Application.Commands;
using SchoolErp.Modules.Examinations.Persistence;
using SchoolErp.Modules.Fees.Api;
using SchoolErp.Modules.Fees.Application.Commands;
using SchoolErp.Modules.Fees.Persistence;
using SchoolErp.Modules.Identity.Api;
using SchoolErp.Modules.Identity.Api.Controllers;
using SchoolErp.Modules.Identity.Application.Commands;
using SchoolErp.Modules.Identity.Persistence;
using SchoolErp.Modules.Inventory.Api;
using SchoolErp.Modules.Inventory.Application.Commands;
using SchoolErp.Modules.Inventory.Persistence;
using SchoolErp.Modules.Library.Api;
using SchoolErp.Modules.Library.Application.Commands;
using SchoolErp.Modules.Library.Persistence;
using SchoolErp.Modules.Students.Api;
using SchoolErp.Modules.Students.Application.Commands;
using SchoolErp.Modules.Students.Persistence;
using SchoolErp.Modules.TenantManagement.Api;
using SchoolErp.Modules.TenantManagement.Api.Controllers;
using SchoolErp.Modules.TenantManagement.Application.Commands;
using SchoolErp.Modules.TenantManagement.Persistence;
using SchoolErp.Modules.Transport.Api;
using SchoolErp.Modules.Transport.Application.Commands;
using SchoolErp.Modules.Transport.Persistence;
using SchoolErp.MultiTenancy;
using SchoolErp.Observability.Health;
using SchoolErp.Observability.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.UseSchoolErpSerilog();

builder.Services.AddSchoolErpHealthChecks(builder.Configuration);
builder.Services.AddSchoolErpMultiTenancy();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

if (builder.Environment.IsDevelopment())
    builder.Services.AddSingleton<IRabbitPublisher, NoOpRabbitPublisher>();
else
    builder.Services.AddSchoolErpEventBus(builder.Configuration);

var moduleAssemblies = new Assembly[]
{
    typeof(CreateTenantCommand).Assembly,
    typeof(CreateUserCommand).Assembly,
    typeof(CreateSubscriptionCommand).Assembly,
    typeof(CreateAcademicYearCommand).Assembly,
    typeof(CreateStudentCommand).Assembly,
    typeof(CreateEmployeeCommand).Assembly,
    typeof(CreateAttendanceSessionCommand).Assembly,
    typeof(CreateExamCommand).Assembly,
    typeof(CreateFeeInvoiceCommand).Assembly,
    typeof(CreateRouteCommand).Assembly,
    typeof(CreateNotificationCommand).Assembly,
    typeof(CreateBookCommand).Assembly,
    typeof(CreateInventoryItemCommand).Assembly,
    typeof(CreateAuditLogCommand).Assembly
};

builder.Services.AddSchoolErpMediatR(moduleAssemblies);

builder.Services.AddTenantManagementApi(builder.Configuration);
builder.Services.AddIdentityApi(builder.Configuration);
builder.Services.AddBillingApi(builder.Configuration);
builder.Services.AddAcademicsApi(builder.Configuration);
builder.Services.AddStudentsApi(builder.Configuration);
builder.Services.AddEmployeesApi(builder.Configuration);
builder.Services.AddAttendanceApi(builder.Configuration);
builder.Services.AddExaminationsApi(builder.Configuration);
builder.Services.AddFeesApi(builder.Configuration);
builder.Services.AddTransportApi(builder.Configuration);
builder.Services.AddCommunicationApi(builder.Configuration);
builder.Services.AddLibraryApi(builder.Configuration);
builder.Services.AddInventoryApi(builder.Configuration);
builder.Services.AddAuditApi(builder.Configuration);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(TenantsController).Assembly)
    .AddApplicationPart(typeof(UsersController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Billing.Api.Controllers.SubscriptionsController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Academics.Api.Controllers.AcademicYearsController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Students.Api.Controllers.StudentsController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Employees.Api.Controllers.EmployeesController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Attendance.Api.Controllers.AttendanceSessionsController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Examinations.Api.Controllers.ExamsController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Fees.Api.Controllers.FeeInvoicesController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Transport.Api.Controllers.RoutesController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Communication.Api.Controllers.NotificationsController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Library.Api.Controllers.BooksController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Inventory.Api.Controllers.InventoryItemsController).Assembly)
    .AddApplicationPart(typeof(SchoolErp.Modules.Audit.Api.Controllers.AuditLogsController).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await EnsureDatabasesCreatedAsync(app.Services);
}

app.MapHealthChecks("/health");
app.UseSchoolErpMultiTenancy();
app.MapControllers();
app.Run();

static async Task EnsureDatabasesCreatedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var contexts = new DbContext[]
    {
        scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>(),
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>(),
        scope.ServiceProvider.GetRequiredService<BillingDbContext>(),
        scope.ServiceProvider.GetRequiredService<AcademicsDbContext>(),
        scope.ServiceProvider.GetRequiredService<StudentsDbContext>(),
        scope.ServiceProvider.GetRequiredService<EmployeesDbContext>(),
        scope.ServiceProvider.GetRequiredService<AttendanceDbContext>(),
        scope.ServiceProvider.GetRequiredService<ExaminationsDbContext>(),
        scope.ServiceProvider.GetRequiredService<FeesDbContext>(),
        scope.ServiceProvider.GetRequiredService<TransportDbContext>(),
        scope.ServiceProvider.GetRequiredService<CommunicationDbContext>(),
        scope.ServiceProvider.GetRequiredService<LibraryDbContext>(),
        scope.ServiceProvider.GetRequiredService<InventoryDbContext>(),
        scope.ServiceProvider.GetRequiredService<AuditDbContext>()
    };

    foreach (var ctx in contexts)
        await ctx.Database.EnsureCreatedAsync();
}
