# Creates School ERP directory layout only (no file content).
$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

$dirs = @(
    "src/Api/SchoolErp.Api/Properties",
    "src/Worker/SchoolErp.Worker",
    "src/Worker/SchoolErp.Hangfire/Jobs",
    "src/BuildingBlocks/BuildingBlocks.Domain/Results",
    "src/BuildingBlocks/BuildingBlocks.Application/Cqrs",
    "src/BuildingBlocks/BuildingBlocks.Application/Behaviors",
    "src/BuildingBlocks/BuildingBlocks.Application/Pagination",
    "src/BuildingBlocks/BuildingBlocks.Application/Modules",
    "src/BuildingBlocks/BuildingBlocks.Application/Persistence",
    "src/BuildingBlocks/BuildingBlocks.Infrastructure/Persistence",
    "src/BuildingBlocks/BuildingBlocks.Infrastructure/Outbox",
    "src/BuildingBlocks/BuildingBlocks.Infrastructure/Caching",
    "src/BuildingBlocks/BuildingBlocks.Infrastructure/EventBus",
    "src/SharedKernel/SchoolErp.SharedKernel/Identifiers",
    "src/MultiTenancy/SchoolErp.MultiTenancy/Middleware",
    "src/MultiTenancy/SchoolErp.MultiTenancy/Resolvers",
    "src/EventBus/SchoolErp.EventBus/RabbitMq",
    "src/EventBus/SchoolErp.EventBus/Contracts/Tenant",
    "src/Authorization/SchoolErp.Authorization",
    "src/Observability/SchoolErp.Observability/Health",
    "src/Observability/SchoolErp.Observability/Logging",
    "src/Database/SchoolErp.Migrator",
    "src/Database/Scripts"
)

$moduleNames = @(
    "TenantManagement", "Identity", "Billing", "Academics", "Students", "Employees",
    "Attendance", "Examinations", "Fees", "Transport", "Communication", "Library",
    "Inventory", "Audit"
)

$layers = @("Domain/Aggregates", "Domain/Repositories", "Application/Commands", "Persistence", "Infrastructure", "Api/Controllers")

foreach ($module in $moduleNames) {
    foreach ($layer in $layers) {
        $dirs += "src/Modules/$module/$module.$layer".Replace("/$module.Persistence", "/$module.Persistence/Repositories")
    }
    $dirs += "src/Modules/$module/$module.Domain"
    $dirs += "src/Modules/$module/$module.Application"
    $dirs += "src/Modules/$module/$module.Persistence"
    $dirs += "src/Modules/$module/$module.Infrastructure"
    $dirs += "src/Modules/$module/$module.Api"
}

foreach ($d in $dirs) {
    $path = Join-Path $root $d
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
        Write-Host "Created $d"
    }
}

Write-Host "Scaffold complete under $root"
