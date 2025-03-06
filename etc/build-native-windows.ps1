# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

# Define the matrix of runtime identifiers to build for using separate arrays
$ridArray = @("win-x64", "win-arm64")
$artifactNameArray = @("stellaris-sav-win-x64", "stellaris-sav-win-arm64")
$binaryNameArray = @("stellaris-sav.exe", "stellaris-sav.exe")
$sourceBinaryArray = @("StellarisSaveParser.Cli.exe", "StellarisSaveParser.Cli.exe")


# https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile#windows
# Function to build a native executable for a specific runtime identifier
function Build-NativeExecutable {
    param (
        [string]$rid,
        [string]$artifactName,
        [string]$binaryName,
        [string]$sourceBinary
    )
    
    Write-Host "`n========================================================"
    Write-Host "Building native executable for $rid..."
    Write-Host "========================================================"
    
    # Create output directory
    $outputDir = "./native-build/$rid"
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
    
    # Build the native executable
    $buildResult = dotnet publish ./StellarisSaveParser.Cli/StellarisSaveParser.Cli.csproj `
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
            
            # Create zip file
            $zipPath = Join-Path $outputDir "$artifactName.zip"
            Compress-Archive -Path $targetPath -DestinationPath $zipPath -Force
            Write-Host "Created zip file: $zipPath"
            
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
    
    Write-Host "Building with:"
    Write-Host "  RID: $rid"
    Write-Host "  Artifact: $artifactName"
    Write-Host "  Binary: $binaryName"
    Write-Host "  Source: $sourceBinary"
    
    if (Build-NativeExecutable $rid $artifactName $binaryName $sourceBinary) {
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