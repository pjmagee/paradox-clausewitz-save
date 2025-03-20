#!/usr/bin/env pwsh
param (
    [string]$Configuration = "Release"
)

$ProjectPath = "../MageeSoft.Paradox.Clausewitz.Save.Cli/MageeSoft.Paradox.Clausewitz.Save.Cli.csproj"

# Show GitVersion info
Write-Host "Retrieving version information from GitVersion..." -ForegroundColor Cyan
dotnet gitversion | Out-Host

Write-Host "Building .NET Tool package..." -ForegroundColor Cyan

# Build the .NET Tool package
dotnet pack $ProjectPath `
    -c $Configuration `
    /p:PackAsTool=true

if ($LASTEXITCODE -eq 0) {
    $NupkgDir = "../MageeSoft.Paradox.Clausewitz.Save.Cli/nupkg"
    
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
            Write-Host "dotnet tool install --global --add-source $NupkgDir MageeSoft.Paradox.Clausewitz.Save.Cli" -ForegroundColor Yellow
            
            Write-Host "`nTo update the tool if already installed:" -ForegroundColor Yellow
            Write-Host "dotnet tool update --global --add-source $NupkgDir MageeSoft.Paradox.Clausewitz.Save.Cli" -ForegroundColor Yellow
        } else {
            Write-Host "Build completed but no packages found in $NupkgDir" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Build completed but nupkg directory not found: $NupkgDir" -ForegroundColor Yellow
    }
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
} 