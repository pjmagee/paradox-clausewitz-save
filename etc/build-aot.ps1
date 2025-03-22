#!/usr/bin/env pwsh
param (
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [string]$VersionPrefix = "",
    [string]$VersionSuffix = ""
)

$ProjectPath = "../src/MageeSoft.Paradox.Clausewitz.Save.Cli/MageeSoft.Paradox.Clausewitz.Save.Cli.csproj"
$AotOutputDir = "../bin/aot-$RuntimeIdentifier"

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
dotnet publish $ProjectPath `
    -c $Configuration `
    -r $RuntimeIdentifier `
    -o $AotOutputDir `
    $VersionArgs

if ($LASTEXITCODE -eq 0) {
    $ExeName = if ($RuntimeIdentifier.StartsWith("win")) { "MageeSoft.Paradox.Clausewitz.Save.Cli.exe" } else { "MageeSoft.Paradox.Clausewitz.Save.Cli" }
    $ExePath = Join-Path -Path $AotOutputDir -ChildPath $ExeName
    
    if (Test-Path $ExePath) {
        $FileInfo = Get-Item $ExePath
        $FileSizeMB = [math]::Round($FileInfo.Length / 1MB, 2)
        
        Write-Host "`nBuild completed successfully!" -ForegroundColor Green
        Write-Host "Output: $ExePath" -ForegroundColor Green
        Write-Host "Size: $FileSizeMB MB" -ForegroundColor Green
        
        # Get version info
        if ($RuntimeIdentifier.StartsWith("win")) {
            $VersionInfo = (Get-Item $ExePath).VersionInfo
            Write-Host "Version: $($VersionInfo.ProductVersion)" -ForegroundColor Green
        }

        # Run version command
        Write-Host "`nExecuting info command:" -ForegroundColor Cyan
        & $ExePath info
    } else {
        Write-Host "Build completed but executable not found at expected location: $ExePath" -ForegroundColor Yellow
    }
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
} 