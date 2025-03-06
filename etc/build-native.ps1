# Set error action preference to stop on errors
$ErrorActionPreference = "Stop"

# Define the matrix of runtime identifiers to build for
$matrix = @(
    @{
        rid = "win-x64"
        artifact_name = "stellaris-sav-win-x64"
        binary_name = "stellaris-sav.exe"
        source_binary = "StellarisSaveParser.Cli.exe"
    },
    @{
        rid = "win-arm64"
        artifact_name = "stellaris-sav-win-arm64"
        binary_name = "stellaris-sav.exe"
        source_binary = "StellarisSaveParser.Cli.exe"
    },
    @{
        rid = "linux-x64"
        artifact_name = "stellaris-sav-linux-x64"
        binary_name = "stellaris-sav"
        source_binary = "StellarisSaveParser.Cli"
    },
    @{
        rid = "linux-arm64"
        artifact_name = "stellaris-sav-linux-arm64"
        binary_name = "stellaris-sav"
        source_binary = "StellarisSaveParser.Cli"
    },
    @{
        rid = "osx-x64"
        artifact_name = "stellaris-sav-osx-x64"
        binary_name = "stellaris-sav"
        source_binary = "StellarisSaveParser.Cli"
    },
    @{
        rid = "osx-arm64"
        artifact_name = "stellaris-sav-osx-arm64"
        binary_name = "stellaris-sav"
        source_binary = "StellarisSaveParser.Cli"
    }
)

# Function to build a native executable for a specific runtime identifier
function Build-NativeExecutable {
    param (
        [Parameter(Mandatory = $true)]
        [hashtable]$Target
    )

    $rid = $Target.rid
    $artifactName = $Target.artifact_name
    $binaryName = $Target.binary_name
    $sourceBinary = $Target.source_binary

    Write-Host "`n========================================================" -ForegroundColor Cyan
    Write-Host "Building native executable for $rid..." -ForegroundColor Cyan
    Write-Host "========================================================" -ForegroundColor Cyan

    # Create output directory if it doesn't exist
    $outputDir = "../native-build/$rid"
    if (!(Test-Path -Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }

    # Build the native executable
    dotnet publish ../StellarisSaveParser.Cli/StellarisSaveParser.Cli.csproj `
        -c Release `
        -r $rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=true `
        -o $outputDir

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nNative build completed successfully for $rid!" -ForegroundColor Green
        
        $exePath = Join-Path -Path $outputDir -ChildPath $sourceBinary
        
        if (Test-Path $exePath) {
            # Rename the executable
            $targetPath = Join-Path -Path $outputDir -ChildPath $binaryName
            Move-Item -Path $exePath -Destination $targetPath -Force
            Write-Host "Renamed $sourceBinary to $binaryName" -ForegroundColor Green
            
            # Get file size
            $fileSize = (Get-Item $targetPath).Length / 1MB
            Write-Host "Executable size: $($fileSize.ToString('0.00')) MB" -ForegroundColor Cyan
            
            # Create zip file
            $zipPath = Join-Path -Path $outputDir -ChildPath "$artifactName.zip"
            Compress-Archive -Path $targetPath -DestinationPath $zipPath -Force
            Write-Host "Created zip file: $zipPath" -ForegroundColor Green
            
            return $true
        } else {
            Write-Host "Executable not found at expected path: $exePath" -ForegroundColor Red
            
            Write-Host "`nListing files in output directory:" -ForegroundColor Yellow
            Get-ChildItem -Path $outputDir | ForEach-Object {
                Write-Host "  $($_.Name)"
            }
            
            return $false
        }
    } else {
        Write-Host "`nNative build failed for $rid with exit code $LASTEXITCODE" -ForegroundColor Red
        return $false
    }
}

# Build for all targets in the matrix
$successCount = 0
$failureCount = 0

# Determine current OS
$currentOS = "unknown"
if ($IsWindows -or $PSVersionTable.OS -match "Windows") {
    $currentOS = "windows"
} elseif ($IsLinux -or $PSVersionTable.OS -match "Linux") {
    $currentOS = "linux"
} elseif ($IsMacOS -or $PSVersionTable.OS -match "Darwin") {
    $currentOS = "macos"
}

Write-Host "Detected OS: $currentOS" -ForegroundColor Cyan

# Parse command line arguments
$targetPlatform = $null
$forceAllPlatforms = $false

for ($i = 0; $i -lt $args.Count; $i++) {
    if ($args[$i] -eq "-Platform" -or $args[$i] -eq "--platform") {
        if ($i + 1 -lt $args.Count) {
            $targetPlatform = $args[$i + 1]
            $i++
        }
    }
    elseif ($args[$i] -eq "-ForceAll" -or $args[$i] -eq "--force-all") {
        $forceAllPlatforms = $true
    }
}

# Determine build strategy
if ($null -ne $targetPlatform) {
    Write-Host "Building only for platform: $targetPlatform" -ForegroundColor Yellow
}
elseif ($forceAllPlatforms) {
    Write-Host "WARNING: Forcing build for ALL platforms regardless of current OS" -ForegroundColor Red
    Write-Host "This may fail due to cross-compilation limitations. See: https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile" -ForegroundColor Red
}
else {
    Write-Host "Building only for current OS: $currentOS" -ForegroundColor Yellow
    Write-Host "Use -ForceAll to attempt building for all platforms (may fail)" -ForegroundColor Yellow
    
    # Map OS name to RID prefix
    if ($currentOS -eq "windows") {
        $targetPlatform = "win"
    } elseif ($currentOS -eq "macos") {
        $targetPlatform = "osx"
    } else {
        $targetPlatform = $currentOS
    }
}

foreach ($target in $matrix) {
    $rid = $target.rid
    
    # Skip if not matching target platform or current OS
    if ($null -ne $targetPlatform -and -not $rid.StartsWith($targetPlatform)) {
        Write-Host "`nSkipping $rid (not matching target platform $targetPlatform)" -ForegroundColor Yellow
        continue
    }
    
    $result = Build-NativeExecutable -Target $target
    
    if ($result) {
        $successCount++
    } else {
        $failureCount++
    }
}

# Print summary
Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host "Build Summary" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Successful builds: $successCount" -ForegroundColor Green
Write-Host "Failed builds: $failureCount" -ForegroundColor $(if ($failureCount -gt 0) { "Red" } else { "Green" })
Write-Host "`nNative executables are available in the ../native-build directory" -ForegroundColor Cyan

# Return success if all builds succeeded
if ($failureCount -gt 0) {
    exit 1
} else {
    exit 0
} 