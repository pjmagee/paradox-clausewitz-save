param(
    [string]$ProjectPath = (Join-Path -Path ".." -ChildPath "src/MageeSoft.Paradox.Clausewitz.Save.Cli"),
    [switch]$Clean,
    [string]$CommandToTest = "info"
)

# Normalize paths for cross-platform compatibility
$ProjectPath = (Resolve-Path $ProjectPath).Path
$OutputDir = Join-Path -Path $ProjectPath -ChildPath "bin"
$NupkgDir = Join-Path -Path $ProjectPath -ChildPath "nupkg"
$ToolManifestPath = Join-Path -Path (Get-Location) -ChildPath ".config/dotnet-tools.json"
$ObjDir = Join-Path -Path $ProjectPath -ChildPath "obj"
$PackageId = "mageesoft.paradox.clausewitz.save.cli"
$ToolCommand = "paradox-clausewitz-sav"

# Show configuration
Write-Host "Test Tool Configuration:" -ForegroundColor Cyan
Write-Host "  Project Path: $ProjectPath" -ForegroundColor Yellow
Write-Host "  Clean: $Clean" -ForegroundColor Yellow
Write-Host "  Command to test: $CommandToTest" -ForegroundColor Yellow

# Always clean the output directory for consistency
Write-Host "`nCleaning output directories..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir -ErrorAction SilentlyContinue
}

# Clean obj directory too to ensure a clean build
if (Test-Path $ObjDir) {
    Remove-Item -Recurse -Force $ObjDir -ErrorAction SilentlyContinue
}

# Clean the nupkg directory to avoid confusion with previous packages
if (Test-Path $NupkgDir) {
    Write-Host "Cleaning package directory..." -ForegroundColor Yellow
    Remove-Item -Path "$NupkgDir/*" -Recurse -Force -ErrorAction SilentlyContinue
}

# Create nupkg directory if it doesn't exist
if (-not (Test-Path $NupkgDir)) {
    New-Item -ItemType Directory -Path $NupkgDir | Out-Null
}

# Create tool manifest if it doesn't exist
if (-not (Test-Path $ToolManifestPath)) {
    Write-Host "`nCreating tool manifest..." -ForegroundColor Cyan
    dotnet new tool-manifest
}

# Build the project - force a clean rebuild
Write-Host "`nBuilding the project..." -ForegroundColor Cyan
dotnet build $ProjectPath -c Release --no-incremental

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Pack the tool - let GitVersion determine the version, forcing a rebuild
Write-Host "`nPacking as .NET tool..." -ForegroundColor Cyan
dotnet pack $ProjectPath -c Release -p:PackAsTool=true -p:IncludeSymbols=true --output $NupkgDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Pack failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Find the package file
$packageFiles = Get-ChildItem -Path $NupkgDir -Filter "*.nupkg" | Where-Object { -not $_.Name.EndsWith('.symbols.nupkg') }
if ($packageFiles.Count -eq 0) {
    Write-Error "No package files found in $NupkgDir"
    exit 1
}

$packageFile = $packageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$packageFileName = $packageFile.Name
Write-Host "`nCreated package: $packageFileName" -ForegroundColor Green

# Extract the exact version from the filename
$packageNamePattern = "(?i)MageeSoft\.Paradox\.Clausewitz\.Save\.Cli\.(.+?)\.nupkg"
if ($packageFileName -match $packageNamePattern) {
    $actualVersion = $matches[1]
    Write-Host "Extracted version: $actualVersion" -ForegroundColor Yellow
} else {
    Write-Error "Failed to extract version from package filename: $packageFileName"
    exit 1
}

# Uninstall any existing tool version
Write-Host "`nUninstalling any existing tool version..." -ForegroundColor Cyan
dotnet tool uninstall --local $PackageId 2>&1 | Out-Null

# Install the tool with the exact version from the package
Write-Host "`nInstalling the tool with version $actualVersion..." -ForegroundColor Cyan
dotnet tool install --local --add-source $NupkgDir $PackageId --version $actualVersion

if ($LASTEXITCODE -ne 0) {
    Write-Error "Tool installation failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Show installed tools
Write-Host "`nInstalled tools:" -ForegroundColor Cyan
dotnet tool list --local

# Test the tool
Write-Host "`nTesting the tool..." -ForegroundColor Cyan
Write-Host "Running: dotnet tool run $ToolCommand -- --version" -ForegroundColor Yellow
dotnet tool run $ToolCommand -- --version

if ($CommandToTest) {
    Write-Host "`nRunning: dotnet tool run $ToolCommand -- $CommandToTest" -ForegroundColor Yellow
    dotnet tool run $ToolCommand -- $CommandToTest
}

# Uninstall the tool after testing
Write-Host "`nUninstalling the tool..." -ForegroundColor Cyan
dotnet tool uninstall --local $PackageId

Write-Host "`nTest completed successfully!" -ForegroundColor Green