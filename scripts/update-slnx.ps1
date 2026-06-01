# Merges School ERP projects into Backend.slnx without removing LMS entries.
$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$slnxPath = Join-Path $root "Backend.slnx"

$existing = @()
if (Test-Path $slnxPath) {
    $existing = Select-String -Path $slnxPath -Pattern 'Path="([^"]+)"' -AllMatches |
        ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value }
}

$schoolErpProjects = Get-ChildItem -Path (Join-Path $root "src") -Recurse -Filter "*.csproj" |
    Where-Object {
        $_.FullName -notmatch '\\Services\\' -and
        $_.Name -notmatch '^LMS\.' -and
        $_.FullName -notmatch 'LMS\.SharedKernal' -and
        $_.FullName -notmatch 'LMS\.EventBus'
    } |
    ForEach-Object {
        $_.FullName.Substring($root.Length + 1).Replace('\', '/')
    } |
    Sort-Object -Unique

$allProjects = ($existing + $schoolErpProjects) | Sort-Object -Unique

$lines = @("<Solution>")
foreach ($p in $allProjects) {
    $lines += "  <Project Path=`"$p`" />"
}
$lines += "</Solution>"

Set-Content -Path $slnxPath -Value ($lines -join "`n") -Encoding UTF8
Write-Host "Updated Backend.slnx with $($allProjects.Count) projects."
