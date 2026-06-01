# Generates 14 School ERP modules (Domain through Api).
$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

$modules = @(
    @{ Module = "TenantManagement"; Agg = "Tenant"; Table = "Tenants"; Prop = "Name"; ExtraProps = @("Code") }
    @{ Module = "Identity";         Agg = "User";   Table = "Users";   Prop = "Email"; ExtraProps = @() }
    @{ Module = "Billing";          Agg = "Subscription"; Table = "Subscriptions"; Prop = "PlanCode"; ExtraProps = @() }
    @{ Module = "Academics";        Agg = "AcademicYear"; Table = "AcademicYears"; Prop = "Name"; ExtraProps = @("StartDate", "EndDate") }
    @{ Module = "Students";         Agg = "Student"; Table = "Students"; Prop = "FirstName"; ExtraProps = @("LastName") }
    @{ Module = "Employees";        Agg = "Employee"; Table = "Employees"; Prop = "FirstName"; ExtraProps = @("LastName") }
    @{ Module = "Attendance";       Agg = "AttendanceSession"; Table = "AttendanceSessions"; Prop = "SessionDate"; ExtraProps = @() }
    @{ Module = "Examinations";     Agg = "Exam"; Table = "Exams"; Prop = "Title"; ExtraProps = @() }
    @{ Module = "Fees";             Agg = "FeeInvoice"; Table = "FeeInvoices"; Prop = "InvoiceNumber"; ExtraProps = @() }
    @{ Module = "Transport";        Agg = "Route"; Table = "Routes"; Prop = "Name"; ExtraProps = @() }
    @{ Module = "Communication";    Agg = "Notification"; Table = "Notifications"; Prop = "Subject"; ExtraProps = @() }
    @{ Module = "Library";          Agg = "Book"; Table = "Books"; Prop = "Title"; ExtraProps = @("Isbn") }
    @{ Module = "Inventory";        Agg = "InventoryItem"; Table = "InventoryItems"; Prop = "Sku"; ExtraProps = @("Name") }
    @{ Module = "Audit";            Agg = "AuditLog"; Table = "AuditLogs"; Prop = "Action"; ExtraProps = @() }
)

function Write-File($relativePath, [string]$content) {
    $full = Join-Path $root $relativePath
    $dir = Split-Path $full -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Set-Content -Path $full -Value $content -Encoding UTF8
    Write-Host "  $relativePath"
}

foreach ($m in $modules) {
    $mod = $m.Module
    $agg = $m.Agg
    $table = $m.Table
    $prop = $m.Prop
    $extras = $m.ExtraProps
    $isTenant = $mod -eq "TenantManagement"

    $aggLower = $agg.Substring(0,1).ToLower() + $agg.Substring(1)
    $repoInterface = "I${agg}Repository"
    $createCmd = "Create${agg}Command"
    $createValidator = "${createCmd}Validator"
    $controller = "${agg}sController"
    if ($agg -eq "User") { $controller = "UsersController" }
    if ($agg -eq "AttendanceSession") { $controller = "AttendanceSessionsController" }
    if ($agg -eq "AcademicYear") { $controller = "AcademicYearsController" }
    if ($agg -eq "FeeInvoice") { $controller = "FeeInvoicesController" }
    if ($agg -eq "AuditLog") { $controller = "AuditLogsController" }
    if ($agg -eq "InventoryItem") { $controller = "InventoryItemsController" }
    if ($agg -eq "Notification") { $controller = "NotificationsController" }
    if ($agg -eq "Subscription") { $controller = "SubscriptionsController" }
    if ($agg -eq "Tenant") { $controller = "TenantsController" }
    if ($agg -eq "Student") { $controller = "StudentsController" }
    if ($agg -eq "Employee") { $controller = "EmployeesController" }
    if ($agg -eq "Exam") { $controller = "ExamsController" }
    if ($agg -eq "Route") { $controller = "RoutesController" }
    if ($agg -eq "Book") { $controller = "BooksController" }

    Write-Host "Module: $mod"

    # --- Domain csproj ---
    Write-File "src/Modules/$mod/$mod.Domain/$mod.Domain.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SchoolErp.Modules.$mod.Domain</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\BuildingBlocks.Domain\BuildingBlocks.Domain.csproj" />
  </ItemGroup>
</Project>
"@

    # --- Aggregate ---
    $extraFields = ""
    $extraCtorParams = ""
    $extraCtorAssign = ""
    $extraCreateParams = ""
    $extraCreateArgs = ""
    $extraPropsPublic = ""
    foreach ($ep in $extras) {
        $t = "string"
        if ($ep -match "Date") { $t = "DateOnly" }
        if ($ep -eq "Isbn") { $t = "string?" }
        $extraFields += "`n    public $t $ep { get; private set; }"
        $extraCtorParams += ", $t $ep"
        $extraCtorAssign += "`n        $ep = $ep;"
        $extraCreateParams += ", $t $ep"
        $extraCreateArgs += ", $ep"
        $extraPropsPublic += "`n        $ep = $ep,"
    }

    $provisionMethod = ""
    $tenantIdOnEntity = "public Guid TenantId { get; private set; }"
    $createFactory = @"
    public static ${agg} Create(Guid tenantId, string $prop$extraCreateParams)
    {
        return new ${agg}(Guid.NewGuid(), tenantId, $prop$extraCreateArgs);
    }
"@
    if ($isTenant) {
        $tenantIdOnEntity = ""
        $provisionMethod = @"

    public string Code { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";

    public void Provision()
    {
        if (Status == "Provisioned")
            throw new DomainException("Tenant is already provisioned.");
        Status = "Provisioned";
    }
"@
        $createFactory = @"
    public static ${agg} Create(string name, string code)
    {
        return new ${agg}(Guid.NewGuid(), name, code);
    }
"@
        $extraFields = "`n    public string Code { get; private set; } = string.Empty;`n    public string Status { get; private set; } = `"Pending`";"
    }

    $privateCtor = if ($isTenant) {
        "private ${agg}(Guid id, string name, string code)`n    {`n        Id = id;`n        Name = name;`n        Code = code;`n    }"
    } else {
        "private ${agg}(Guid id, Guid tenantId, string $prop$extraCtorParams)`n    {`n        Id = id;`n        TenantId = tenantId;`n        $prop = $prop;$extraCtorAssign`n    }"
    }

    Write-File "src/Modules/$mod/$mod.Domain/Aggregates/$agg.cs" @"
using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.$mod.Domain.Aggregates;

public sealed class ${agg} : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    $tenantIdOnEntity
    $extraFields

    $privateCtor

    $createFactory
    $provisionMethod
}
"@

    # --- Repository interface ---
    Write-File "src/Modules/$mod/$mod.Domain/Repositories/${repoInterface}.cs" @"
using SchoolErp.Modules.$mod.Domain.Aggregates;

namespace SchoolErp.Modules.$mod.Domain.Repositories;

public interface ${repoInterface}
{
    Task AddAsync(${agg} entity, CancellationToken cancellationToken = default);
    Task<${agg}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
"@

    # --- Application csproj ---
    Write-File "src/Modules/$mod/$mod.Application/$mod.Application.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SchoolErp.Modules.$mod.Application</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.11.0" />
    <PackageReference Include="MediatR" Version="12.4.1" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\BuildingBlocks.Application\BuildingBlocks.Application.csproj" />
    <ProjectReference Include="..\$mod.Domain\$mod.Domain.csproj" />
  </ItemGroup>
</Project>
"@

    function Get-CsType([string]$name) {
        if ($name -match 'Date$') { return 'DateOnly' }
        if ($name -eq 'Isbn') { return 'string?' }
        return 'string'
    }

    # --- Command ---
    $cmdProps = if ($isTenant) {
        "string Name, string Code"
    } else {
        $parts = @("Guid TenantId", "string $prop")
        foreach ($ep in $extras) { $parts += "$(Get-CsType $ep) $ep" }
        $parts -join ', '
    }
    $cmdRecord = "public sealed record ${createCmd}($cmdProps) : ICommand<Result<Guid>>;"

    $handlerBody = if ($isTenant) {
        @"
        var entity = ${agg}.Create(request.Name, request.Code);
        entity.Provision();
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
"@
    } else {
        $createCall = "${agg}.Create(request.TenantId, request.$prop"
        foreach ($ep in $extras) { $createCall += ", request.$ep" }
        $createCall += ")"
        @"
        var entity = $createCall;
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
"@
    }

    Write-File "src/Modules/$mod/$mod.Application/Commands/${createCmd}.cs" @"
using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.$mod.Domain.Aggregates;
using SchoolErp.Modules.$mod.Domain.Repositories;

namespace SchoolErp.Modules.$mod.Application.Commands;

$cmdRecord

public sealed class ${createCmd}Handler : IRequestHandler<${createCmd}, Result<Guid>>
{
    private readonly ${repoInterface} _repository;

    public ${createCmd}Handler(${repoInterface} repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(${createCmd} request, CancellationToken cancellationToken)
    {
$handlerBody
    }
}
"@

    $validatorRules = if ($isTenant) {
        @"
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
"@
    } else {
        "        RuleFor(x => x.TenantId).NotEmpty();`n        RuleFor(x => x.$prop).NotEmpty();"
    }

    Write-File "src/Modules/$mod/$mod.Application/Commands/${createValidator}.cs" @"
using FluentValidation;

namespace SchoolErp.Modules.$mod.Application.Commands;

public sealed class ${createValidator} : AbstractValidator<${createCmd}>
{
    public ${createValidator}()
    {
$validatorRules
    }
}
"@

    # --- Persistence ---
    Write-File "src/Modules/$mod/$mod.Persistence/$mod.Persistence.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SchoolErp.Modules.$mod.Persistence</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\BuildingBlocks.Infrastructure\BuildingBlocks.Infrastructure.csproj" />
    <ProjectReference Include="..\$mod.Domain\$mod.Domain.csproj" />
  </ItemGroup>
</Project>
"@

    $dbSetLine = "    public DbSet<${agg}> $table => Set<${agg}>();"
    $onModelConfig = if ($isTenant) {
        @"
        modelBuilder.Entity<${agg}>(e =>
        {
            e.ToTable("$table", "$mod");
            e.Property(x => x.Id).HasColumnName("TenantId");
            e.HasKey(x => x.Id);
        });
"@
    } else {
        @"
        modelBuilder.Entity<${agg}>(e =>
        {
            e.ToTable("$table", "$mod");
            e.HasKey(x => x.Id);
        });
"@
    }

    Write-File "src/Modules/$mod/$mod.Persistence/${mod}DbContext.cs" @"
using Microsoft.EntityFrameworkCore;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.$mod.Domain.Aggregates;

namespace SchoolErp.Modules.$mod.Persistence;

public sealed class ${mod}DbContext : BaseDbContext
{
    public ${mod}DbContext(DbContextOptions<${mod}DbContext> options) : base(options) { }

$dbSetLine

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
$onModelConfig
    }
}
"@

    Write-File "src/Modules/$mod/$mod.Persistence/Repositories/${agg}Repository.cs" @"
using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.$mod.Domain.Aggregates;
using SchoolErp.Modules.$mod.Domain.Repositories;

namespace SchoolErp.Modules.$mod.Persistence.Repositories;

public sealed class ${agg}Repository : ${repoInterface}
{
    private readonly ${mod}DbContext _db;

    public ${agg}Repository(${mod}DbContext db) => _db = db;

    public async Task AddAsync(${agg} entity, CancellationToken cancellationToken = default)
    {
        await _db.$table.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<${agg}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.$table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
"@

    # --- Infrastructure ---
    Write-File "src/Modules/$mod/$mod.Infrastructure/$mod.Infrastructure.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SchoolErp.Modules.$mod.Infrastructure</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\$mod.Application\$mod.Application.csproj" />
    <ProjectReference Include="..\$mod.Persistence\$mod.Persistence.csproj" />
  </ItemGroup>
</Project>
"@

    Write-File "src/Modules/$mod/$mod.Infrastructure/${mod}ModuleExtensions.cs" @"
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.BuildingBlocks.Infrastructure.Persistence;
using SchoolErp.Modules.$mod.Domain.Repositories;
using SchoolErp.Modules.$mod.Persistence;
using SchoolErp.Modules.$mod.Persistence.Repositories;

namespace SchoolErp.Modules.$mod.Infrastructure;

public static class ${mod}ModuleExtensions
{
    public static IServiceCollection Add${mod}Module(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<${mod}DbContext>(options =>
            options.ConfigureSchoolErpDatabase(configuration, "$mod"));

        services.AddScoped<${repoInterface}, ${agg}Repository>();
        return services;
    }
}
"@

    # --- Api ---
    Write-File "src/Modules/$mod/$mod.Api/$mod.Api.csproj" @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SchoolErp.Modules.$mod.Api</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\$mod.Application\$mod.Application.csproj" />
    <ProjectReference Include="..\$mod.Infrastructure\$mod.Infrastructure.csproj" />
  </ItemGroup>
</Project>
"@

    $requestProps = if ($isTenant) { "string Name, string Code" } else {
        $parts = @("Guid TenantId", "string $prop")
        foreach ($ep in $extras) { $parts += "$(Get-CsType $ep) $ep" }
        $parts -join ', '
    }
    $requestToCmd = if ($isTenant) { "new ${createCmd}(request.Name, request.Code)" } else {
        $c = "new ${createCmd}(request.TenantId, request.$prop"
        foreach ($ep in $extras) { $c += ", request.$ep" }
        "$c)"
    }
    $routePrefix = $mod.ToLowerInvariant()
    if ($mod -eq "TenantManagement") { $routePrefix = "tenants" }

    Write-File "src/Modules/$mod/$mod.Api/${mod}ApiExtensions.cs" @"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Modules.$mod.Infrastructure;

namespace SchoolErp.Modules.$mod.Api;

public static class ${mod}ApiExtensions
{
    public static IServiceCollection Add${mod}Api(this IServiceCollection services, IConfiguration configuration) =>
        services.Add${mod}Module(configuration);
}
"@

    Write-File "src/Modules/$mod/$mod.Api/Controllers/$controller.cs" @"
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.$mod.Application.Commands;

namespace SchoolErp.Modules.$mod.Api.Controllers;

[ApiController]
[Route("api/v1/$routePrefix")]
public sealed class $controller : ControllerBase
{
    private readonly ISender _sender;

    public $controller(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] Create${agg}Request request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send($requestToCmd, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record Create${agg}Request($requestProps);
"@
}

Write-Host "Module generation complete."
