#!/usr/bin/env pwsh
param (
    [string]$Configuration = "Release"
)

# Use Join-Path for cross-platform path handling
$RootDir = Join-Path -Path (Get-Location) -ChildPath ".."
$SrcDir = Join-Path -Path $RootDir -ChildPath "src"
$SolutionPath = Join-Path -Path $SrcDir -ChildPath "MageeSoft.PDX.CE.slnx"
$ProjectPath = Join-Path -Path $SrcDir -ChildPath "MageeSoft.PDX.CE.Cli"
$CsprojPath = Join-Path -Path $ProjectPath -ChildPath "MageeSoft.PDX.CE.Cli.csproj"

# Check if GitVersion.Tool is installed
if (-not (dotnet tool list --global | Select-String -Pattern "gitversion")) {
    Write-Host "GitVersion.Tool not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global GitVersion.Tool
} else {
    Write-Host "GitVersion.Tool is already installed." -ForegroundColor Green
}

# Show GitVersion info
Write-Host "Retrieving version information from GitVersion..." -ForegroundColor Cyan
dotnet gitversion | Out-Host

# First restore solution dependencies
Write-Host "Restoring dependencies for the solution..." -ForegroundColor Cyan
dotnet restore $SolutionPath

Write-Host "Building .NET Tool package..." -ForegroundColor Cyan

# Build the .NET Tool package
dotnet pack $CsprojPath `
    -c $Configuration `
    /p:PackAsTool=true

if ($LASTEXITCODE -eq 0) {
    $NupkgDir = Join-Path -Path $ProjectPath -ChildPath "nupkg"
    
    if (Test-Path $NupkgDir) {
        $Packages = Get-ChildItem -Path $NupkgDir -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending
        
        if ($Packages.Count -gt 0) {
            $LatestPackage = $Packages[0]
            $PackageSizeMB = [math]::Round($LatestPackage.Length / 1MB, 2)
            
            Write-Host "`nBuild completed successfully!" -ForegroundColor Green
            Write-Host "Output: $($LatestPackage.FullName)" -ForegroundColor Green
            Write-Host "Size: $PackageSizeMB MB" -ForegroundColor Green
            
            # Show installation instructions
            Write-Host "`nTo install the tool globally:" -ForegroundColor Yellow
            Write-Host "dotnet tool install --global --add-source $NupkgDir MageeSoft.PDX.CE.Cli" -ForegroundColor Yellow
            
            Write-Host "`nTo update the tool if already installed:" -ForegroundColor Yellow
            Write-Host "dotnet tool update --global --add-source $NupkgDir MageeSoft.PDX.CE.Cli" -ForegroundColor Yellow
        } else {
            Write-Host "Build completed but no packages found in $NupkgDir" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Build completed but nupkg directory not found: $NupkgDir" -ForegroundColor Yellow
    }
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
}