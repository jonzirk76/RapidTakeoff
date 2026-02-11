$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================"
Write-Host "[SURE-HIT] PREFLIGHT START"
Write-Host "dotnet restore / build / test (Release)"
Write-Host "========================================"
Write-Host ""

dotnet restore
dotnet build --configuration Release --no-restore
dotnet test  --configuration Release --no-build

Write-Host ""
Write-Host "========================================"
Write-Host "[SURE-HIT] PREFLIGHT PASS"
Write-Host "========================================"
Write-Host ""
