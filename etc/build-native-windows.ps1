# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

# Define the matrix of runtime identifiers to build for using separate arrays
$ridArray = @("win-x64", "win-arm64")
$artifactNameArray = @("paradox-clausewitz-sav-win-x64.exe", "paradox-clausewitz-sav-win-arm64.exe")
$binaryNameArray = @("paradox-clausewitz-sav-win-x64.exe", "paradox-clausewitz-sav-win-arm64.exe")
$sourceBinaryArray = @("MageeSoft.Paradox.Clausewitz.Save.Cli.exe", "MageeSoft.Paradox.Clausewitz.Save.Cli.exe")
$platformArchArray = @("windows/x64", "windows/arm64")

# Make sure GitVersion is installed
if (-not (Get-Command "dotnet-gitversion" -ErrorAction SilentlyContinue)) {
    Write-Host "Installing GitVersion..." -ForegroundColor Cyan
    dotnet tool install --global GitVersion.Tool
}

# Get version info
$gitVersionInfo = dotnet gitversion /output json | ConvertFrom-Json
Write-Host "Building version $($gitVersionInfo.SemVer)" -ForegroundColor Green

# Create artifacts directory
$artifactsDir = Join-Path -Path "." -ChildPath "artifacts"
New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null

# https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile#windows
# Function to build a native executable for a specific runtime identifier
function Build-NativeExecutable {
    param (
        [string]$rid,
        [string]$artifactName,
        [string]$binaryName,
        [string]$sourceBinary,
        [string]$platformArch
    )
    
    Write-Host "`n========================================================"
    Write-Host "Building native executable for $rid..."
    Write-Host "========================================================"
    
    # Parse platform and architecture
    $platform, $arch = $platformArch.Split('/')
    
    # Create output directory
    $outputDir = Join-Path -Path "." -ChildPath "native-build"
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
    
    # Use Join-Path for the project path
    $projectPath = Join-Path -Path "." -ChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli"
    $csprojPath = Join-Path -Path $projectPath -ChildPath "MageeSoft.Paradox.Clausewitz.Save.Cli.csproj"
    
    # Build the native executable
    dotnet publish $csprojPath `
        -c Release `
        -r $rid `
        --self-contained true `
        -p:PublishAot=true `
        -p:StripSymbols=true `
        -p:InvariantGlobalization=true `
        -p:OptimizationPreference=Size `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=true `
        -p:EnableCompressionInSingleFile=true `
        -o $outputDir
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nNative build completed successfully for $rid!"
        
        $exePath = Join-Path $outputDir $sourceBinary
        $targetPath = Join-Path $outputDir $binaryName
        
        if (Test-Path $exePath) {
            # Remove existing target file if it exists
            if (Test-Path $targetPath) {
                Remove-Item -Path $targetPath -Force
            }
            
            # Rename the executable
            Rename-Item -Path $exePath -NewName $binaryName -Force
            Write-Host "Renamed $sourceBinary to $binaryName"
            
            # Get file size
            $fileSize = (Get-Item $targetPath).Length
            Write-Host "Executable size: $([math]::Round($fileSize/1MB, 2)) MB"
            
            # Package using package-native.ps1
            $packageScript = Join-Path -Path $PSScriptRoot -ChildPath "package-native.ps1"
            $packagedPath = & $packageScript -InputFile $targetPath -Platform $platform -Architecture $arch -OutputDir $artifactsDir
            
            if ($packagedPath -and (Test-Path $packagedPath)) {
                Write-Host "Successfully packaged to: $packagedPath" -ForegroundColor Green
            }
            
            return $true
        }
        else {
            Write-Host "`nExecutable not found at expected path: $exePath"
            Write-Host "`nListing files in output directory:"
            Get-ChildItem $outputDir
            
            return $false
        }
    }
    else {
        Write-Host "`nNative build failed for $rid with exit code $LASTEXITCODE"
        return $false
    }
}

# Build for all targets in the matrix
$successCount = 0
$failureCount = 0

# Get the length of the array
$arrayLength = $ridArray.Length
Write-Host "Array length: $arrayLength"
Write-Host "Available RIDs:"
foreach ($rid in $ridArray) {
    Write-Host "  $rid"
}

# Iterate over all RIDs
for ($i = 0; $i -lt $ridArray.Length; $i++) {
    $rid = $ridArray[$i]
    Write-Host "Processing RID: $rid"
    
    $artifactName = $artifactNameArray[$i]
    $binaryName = $binaryNameArray[$i]
    $sourceBinary = $sourceBinaryArray[$i]
    $platformArch = $platformArchArray[$i]
    
    Write-Host "Building with:"
    Write-Host "  RID: $rid"
    Write-Host "  Artifact: $artifactName"
    Write-Host "  Binary: $binaryName"
    Write-Host "  Source: $sourceBinary"
    Write-Host "  Platform/Arch: $platformArch"
    
    if (Build-NativeExecutable $rid $artifactName $binaryName $sourceBinary $platformArch) {
        $successCount++
    }
    else {
        $failureCount++
    }
}

# Print summary
Write-Host "`n========================================================"
Write-Host "Build Summary"
Write-Host "========================================================"
Write-Host "Successful builds: $successCount"
if ($failureCount -gt 0) {
    Write-Host "Failed builds: $failureCount"
    exit 1
}
else {
    Write-Host "Failed builds: $failureCount"
    exit 0
}