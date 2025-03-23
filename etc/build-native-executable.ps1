#!/usr/bin/env pwsh
param (
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [string]$VersionPrefix = "",
    [string]$VersionSuffix = ""
)

# Use Join-Path for cross-platform path handling
$SrcDir = Join-Path -Path ".." -ChildPath "src"
$ProjectPath = Join-Path -Path $SrcDir -ChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli"
$CsprojPath = Join-Path -Path $ProjectPath -ChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli.csproj"
$BinDir = Join-Path -Path ".." -ChildPath "bin"
$AotOutputDir = Join-Path -Path $BinDir -ChildPath "aot-$RuntimeIdentifier"

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

# Create version arguments if provided
$VersionArgs = ""
if ($VersionPrefix) {
    $VersionArgs += " /p:VersionPrefix=$VersionPrefix"
}
if ($VersionSuffix) {
    $VersionArgs += " /p:VersionSuffix=$VersionSuffix"
}

Write-Host "Building Native AOT executable for $RuntimeIdentifier..." -ForegroundColor Cyan

# Build the AOT executable
dotnet publish $CsprojPath `
    -c $Configuration `
    -r $RuntimeIdentifier `
    -o $AotOutputDir `
    $VersionArgs

if ($LASTEXITCODE -eq 0) {
    $ExeName = if ($RuntimeIdentifier.StartsWith("win")) { 
        "MageeSoft.Paradox.Clausewitz.Save.Cli.exe" 
    } else { 
        "MageeSoft.Paradox.Clausewitz.Save.Cli" 
    }

    $ExePath = Join-Path -Path $AotOutputDir -ChildPath $ExeName
    
    if (Test-Path $ExePath) {
        $FileInfo = Get-Item $ExePath
        $FileSizeMB = [math]::Round($FileInfo.Length / 1MB, 2)
        
        Write-Host "`nBuild completed successfully!" -ForegroundColor Green
        Write-Host "Output: $ExePath" -ForegroundColor Green
        Write-Host "Size: $FileSizeMB MB" -ForegroundColor Green
        
        $VersionInfo = (Get-Item $ExePath).VersionInfo
        Write-Host "Version: $($VersionInfo.ProductVersion)" -ForegroundColor Green
        # Run version command
        Write-Host "`nExecuting info command:" -ForegroundColor Cyan
        & $ExePath info
    } else {
        Write-Host "Build completed but executable not found at expected location: $ExePath" -ForegroundColor Yellow
    }
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
}