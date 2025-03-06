# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

# Check if API key is provided
if ($args.Count -eq 0) {
    Write-Host "Error: NuGet API key is required." -ForegroundColor Red
    Write-Host "Usage: .\etc\publish-nuget.ps1 <your-api-key>" -ForegroundColor Yellow
    exit 1
}

$apiKey = $args[0]

Write-Host "Building and packing the tool..." -ForegroundColor Cyan
# Build and pack the tool
dotnet pack ../StellarisSaveParser.Cli/StellarisSaveParser.Cli.csproj -c Release

Write-Host "`nPushing package to NuGet..." -ForegroundColor Cyan
# Push to NuGet
dotnet nuget push "../StellarisSaveParser.Cli/nupkg/MageeSoft.StellarisSaveParser.Cli.*.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json" `
    --skip-duplicate

Write-Host "`nPackage publishing complete!" -ForegroundColor Green
Write-Host "Note: It may take some time for your package to appear on NuGet.org" -ForegroundColor Yellow 