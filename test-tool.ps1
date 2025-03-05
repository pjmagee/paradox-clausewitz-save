# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

Write-Host "Building and packing the tool..." -ForegroundColor Cyan
# Build and pack the tool
dotnet pack StellarisSaveParser.Cli/StellarisSaveParser.Cli.csproj -c Release

Write-Host "`nAttempting to uninstall any existing version..." -ForegroundColor Cyan
# Try to uninstall the tool if it's already installed, but continue if it fails
try {
    dotnet tool uninstall --global MageeSoft.StellarisSaveParser.Cli
} catch {
    Write-Host "Tool was not previously installed or could not be uninstalled. Continuing..." -ForegroundColor Yellow
}

Write-Host "`nInstalling the tool from local package..." -ForegroundColor Cyan
# Install the tool from the local package
dotnet tool install --global --add-source ./StellarisSaveParser.Cli/nupkg MageeSoft.StellarisSaveParser.Cli

# Test the tool
Write-Host "`nTesting the tool..." -ForegroundColor Cyan
Write-Host "`n1. Testing help command:" -ForegroundColor Green
stellaris-sav --help

Write-Host "`n2. Testing version command:" -ForegroundColor Green
stellaris-sav --version

Write-Host "`n3. Testing version subcommand:" -ForegroundColor Green
stellaris-sav version

Write-Host "`n4. Testing list command (if you have Stellaris installed):" -ForegroundColor Green
try {
    stellaris-sav list
} catch {
    Write-Host "List command failed. This is expected if you don't have Stellaris installed." -ForegroundColor Yellow
}

Write-Host "`nTool installation and testing complete!" -ForegroundColor Cyan 

dotnet tool uninstall --global MageeSoft.StellarisSaveParser.Cli