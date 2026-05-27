# LMS API smoke tests (Tenant + Student)
$ErrorActionPreference = "Stop"

$tenantBase = "http://localhost:5011"
$studentBase = "http://localhost:5012"

Write-Host "=== 1. Tenant API health ===" -ForegroundColor Cyan
$health = Invoke-RestMethod -Uri "$tenantBase/health" -Method Get
Write-Host "Health: OK"

Write-Host "`n=== 2. Register tenant ===" -ForegroundColor Cyan
$subdomain = "test-" + [guid]::NewGuid().ToString("N").Substring(0, 8)
$registerBody = @{
    name      = "Test School"
    subdomain = $subdomain
} | ConvertTo-Json

$tenant = Invoke-RestMethod -Uri "$tenantBase/api/tenants" -Method Post -Body $registerBody -ContentType "application/json"
Write-Host "Created tenant: $($tenant.id) subdomain=$($tenant.subdomain)"
$tenantId = $tenant.id

Write-Host "`n=== 3. Get tenant by id ===" -ForegroundColor Cyan
$fetched = Invoke-RestMethod -Uri "$tenantBase/api/tenants/$tenantId" -Method Get
Write-Host "Fetched: $($fetched.name) active=$($fetched.isActive)"

Write-Host "`n=== 4. Student API health ===" -ForegroundColor Cyan
Invoke-RestMethod -Uri "$studentBase/health" -Method Get | Out-Null
Write-Host "Health: OK"

Write-Host "`n=== 5. Create student (with X-Tenant-Id) ===" -ForegroundColor Cyan
$studentBody = @{
    firstName       = "Rahul"
    lastName        = "Kumar"
    admissionNumber = "ADM-" + (Get-Random -Maximum 99999)
} | ConvertTo-Json

$headers = @{ "X-Tenant-Id" = $tenantId }
$student = Invoke-RestMethod -Uri "$studentBase/api/students" -Method Post -Body $studentBody -ContentType "application/json" -Headers $headers
Write-Host "Created student: $($student.id) $($student.firstName) $($student.lastName)"

Write-Host "`n=== 6. Student without tenant (expect 400) ===" -ForegroundColor Cyan
try {
    Invoke-RestMethod -Uri "$studentBase/api/students" -Method Post -Body $studentBody -ContentType "application/json"
    Write-Host "FAIL: expected 400" -ForegroundColor Red
    exit 1
} catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 400) {
        Write-Host "Correctly rejected (400 Bad Request)"
    } else {
        throw
    }
}

Write-Host "`n=== All API smoke tests passed ===" -ForegroundColor Green
