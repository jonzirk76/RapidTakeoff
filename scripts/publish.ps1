$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src/RapidTakeoff.Cli/RapidTakeoff.Cli.csproj"
$configuration = "Release"
$runtime = "win-x64"
$outDir = Join-Path $repoRoot "dist/$runtime"

if (-not (Test-Path -LiteralPath $project)) {
    throw "Project file not found: $project"
}

Write-Host ""
Write-Host "========================================"
Write-Host "[SURE-HIT] PUBLISH START"
Write-Host "Project: $project"
Write-Host "Config:  $configuration"
Write-Host "RID:     $runtime"
Write-Host "Out:     $outDir"
Write-Host "========================================"
Write-Host ""

& dotnet publish $project `
  -c $configuration `
  -r $runtime `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  -o $outDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host ""
Write-Host "========================================"
Write-Host "[SURE-HIT] PUBLISH DONE"
Write-Host "Output: $outDir"
Write-Host "========================================"
Write-Host ""
