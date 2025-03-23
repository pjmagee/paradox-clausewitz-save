# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

# Check if API key is provided
if ($args.Count -eq 0) {
    Write-Host "Error: NuGet API key is required." -ForegroundColor Red
    Write-Host "Usage: pwsh $(Join-Path "etc" "publish-nuget.ps1") <your-api-key>" -ForegroundColor Yellow
    exit 1
}

$apiKey = $args[0]

# Use Join-Path for cross-platform path handling
$projectDir = Join-Path -Path ".." -ChildPath "src"
$projectPath = Join-Path -Path $projectDir -ChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli"
$csprojPath = Join-Path -Path $projectPath -ChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli.csproj"
$nupkgPattern = Join-Path -Path $projectPath -ChildPath "nupkg" -AdditionalChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli.*.nupkg"

Write-Host "Building and packing the tool..." -ForegroundColor Cyan
# Build and pack the tool
dotnet pack $csprojPath -c Release

Write-Host "`nPushing package to NuGet..." -ForegroundColor Cyan
# Push to NuGet
dotnet nuget push $nupkgPattern `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json" `
    --skip-duplicate

Write-Host "`nPackage publishing complete!" -ForegroundColor Green