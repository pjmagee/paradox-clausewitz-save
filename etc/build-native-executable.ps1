#!/usr/bin/env pwsh
param (
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [string]$VersionPrefix = "",
    [string]$VersionSuffix = "",
    [string]$SrcDir  = ""
)

$RootDir = Join-Path -Path $SrcDir -ChildPath ".."
$SolutionPath = Join-Path -Path $SrcDir -ChildPath "MageeSoft.Paradox.Clausewitz.Save.slnx"
$ProjectPath = Join-Path -Path $SrcDir -ChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli"
$CsprojPath = Join-Path -Path $ProjectPath -ChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli.csproj"
$BinDir = Join-Path -Path $RootDir -ChildPath "bin"
$AotOutputDir = Join-Path -Path $BinDir -ChildPath "aot-$RuntimeIdentifier"

# Display paths for debugging
Write-Host "Repository root: $RootDir" -ForegroundColor Cyan
Write-Host "Solution path: $SolutionPath" -ForegroundColor Cyan
Write-Host "Project path: $CsprojPath" -ForegroundColor Cyan
Write-Host "Output directory: $AotOutputDir" -ForegroundColor Cyan

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

# First restore solution dependencies to ensure all projects are correctly resolved
Write-Host "Restoring dependencies for the solution..." -ForegroundColor Cyan
dotnet restore $SolutionPath

Write-Host "Building Native AOT executable for $RuntimeIdentifier..." -ForegroundColor Cyan

# Build the AOT executable
dotnet publish $CsprojPath -c $Configuration -r $RuntimeIdentifier -o $AotOutputDir $VersionArgs

if ($LASTEXITCODE -eq 0) {
    $ExeName = if ($RuntimeIdentifier.StartsWith("win")) { 
        "paradox-clausewitz-sav.exe" 
    } else { 
        "paradox-clausewitz-sav" 
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

        # Check if current system architecture matches the target runtime
        $canRun = $false
        $currentArch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLower()
        
        # Map the runtime identifier to architecture for comparison
        if (($RuntimeIdentifier -like "*x64*" -and $currentArch -eq "x64") -or
            ($RuntimeIdentifier -like "*arm64*" -and $currentArch -eq "arm64") -or
            ($RuntimeIdentifier -like "*x86*" -and $currentArch -eq "x86")) {
            $canRun = $true
        }
        
        if ($canRun) {
            Write-Host "`nExecuting info command:" -ForegroundColor Cyan
            & $ExePath info
        } else {
            Write-Host "`nCannot execute the compiled binary on this system." -ForegroundColor Yellow
            Write-Host "Target architecture ($RuntimeIdentifier) differs from current system architecture ($currentArch)." -ForegroundColor Yellow
        }
    } else {
        Write-Host "Build completed but executable not found at expected location: $ExePath" -ForegroundColor Yellow
    }
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
}