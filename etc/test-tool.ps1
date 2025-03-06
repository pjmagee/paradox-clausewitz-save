$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue" # Speeds up web requests

Write-Host "Building and packing the tool..." -ForegroundColor Cyan
dotnet pack ../StellarisSaveParser.Cli/StellarisSaveParser.Cli.csproj -c Release

# Try to uninstall the tool first, but continue if it fails
try {
    Write-Host "Uninstalling existing tool..." -ForegroundColor Yellow
    dotnet tool uninstall --global MageeSoft.StellarisSaveParser.Cli
}
catch {
    Write-Host "Tool was not previously installed or could not be uninstalled. Continuing..." -ForegroundColor Yellow
}

Write-Host "Installing tool from local package..." -ForegroundColor Cyan
dotnet tool install --global --add-source ../StellarisSaveParser.Cli/nupkg MageeSoft.StellarisSaveParser.Cli

Write-Host "`nTesting commands:" -ForegroundColor Green

# Test help command
Write-Host "`n> stellaris-sav --help" -ForegroundColor Magenta
stellaris-sav --help

# Test version command
Write-Host "`n> stellaris-sav --version" -ForegroundColor Magenta
stellaris-sav --version

# Test list command (with error handling in case Stellaris is not installed)
Write-Host "`n> stellaris-sav list" -ForegroundColor Magenta
try {
    stellaris-sav list
}
catch {
    Write-Host "Error running list command: $_" -ForegroundColor Red
    Write-Host "This is expected if Stellaris is not installed on this machine." -ForegroundColor Yellow
}

Write-Host "`nTesting complete!" -ForegroundColor Green 