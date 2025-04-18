#!/usr/bin/env pwsh
param (
    [Parameter(Mandatory = $true)]
    [string]$InputFile,
    
    [Parameter(Mandatory = $true)]
    [ValidateSet("windows", "linux", "macos")]
    [string]$Platform,
    
    [Parameter(Mandatory = $true)]
    [ValidateSet("x64", "arm64")]
    [string]$Architecture,
    
    [string]$OutputDir = (Join-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath "..") -ChildPath "artifacts")
)

# Resolve output directory to absolute path
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)

# Create output directory if it doesn't exist
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Display output directory for debugging
Write-Host "Artifacts will be placed in: $OutputDir" -ForegroundColor Cyan

# Check if GitVersion.Tool is installed
if (-not (dotnet tool list --global | Select-String -Pattern "gitversion")) {
    Write-Host "GitVersion.Tool not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global GitVersion.Tool
} else {
    Write-Host "GitVersion.Tool is already installed." -ForegroundColor Green
}

# Get version info from GitVersion
Write-Host "Getting version information from GitVersion..." -ForegroundColor Cyan
$gitVersionInfo = dotnet gitversion /output json | ConvertFrom-Json
$versionString = "$($gitVersionInfo.MajorMinorPatch)"

# Set up file paths and names
$baseFileName = "mageesoft-pdx-ce-sav_${versionString}_${Platform}_${Architecture}"
$packageExtension = if ($Platform -eq "windows") { ".zip" } else { ".tar.gz" }
$outputFileName = "$baseFileName$packageExtension"
$outputPath = Join-Path -Path $OutputDir -ChildPath $outputFileName

# Create temporary directory for packaging
$tempRoot = if ($env:TEMP) { $env:TEMP } elseif ($env:TMPDIR) { $env:TMPDIR } else { "/tmp" }
$tempDir = Join-Path -Path $tempRoot -ChildPath ([guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

# Copy the binary to the temporary directory with the desired name
$targetFileName = if ($Platform -eq "windows") { "mageesoft-pdx-ce-sav.exe" } else { "mageesoft-pdx-ce-sav" }
$targetPath = Join-Path -Path $tempDir -ChildPath $targetFileName
Copy-Item -Path $InputFile -Destination $targetPath -Force

# Add a version.txt file with version info
$versionFilePath = Join-Path -Path $tempDir -ChildPath "version.txt"
@"
Version: $($gitVersionInfo.SemVer)
Commit: $($gitVersionInfo.Sha)
Branch: $($gitVersionInfo.BranchName)
Built at: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@ | Out-File -FilePath $versionFilePath -Encoding utf8

# Package the files
Write-Host "Packaging $targetFileName to $outputPath..." -ForegroundColor Cyan
if ($Platform -eq "windows") {
    Compress-Archive -Path (Join-Path -Path $tempDir -ChildPath "*") -DestinationPath $outputPath -Force
} else {
    # For Linux/macOS, use tar to create .tar.gz
    if ($IsWindows) {
        # On Windows, we need to use 7-Zip or another tool to create tar.gz
        if (Get-Command "7z" -ErrorAction SilentlyContinue) {
            Push-Location $tempDir
            7z a -ttar -so archive.tar * | 7z a -tgzip -si $outputPath
            Pop-Location
        } else {
            Write-Warning "7-Zip not found. Install it to create .tar.gz on Windows, or use WSL."
            # Fallback to zip on Windows if 7-Zip is not available
            $outputPath = [System.IO.Path]::ChangeExtension($outputPath, ".zip")
            Compress-Archive -Path (Join-Path -Path $tempDir -ChildPath "*") -DestinationPath $outputPath -Force
        }
    } else {
        # On Linux/macOS, use native tar command
        tar -czf $outputPath -C $tempDir .
    }
}

# Clean up
Remove-Item -Path $tempDir -Recurse -Force

# Output success message
if (Test-Path $outputPath) {
    $fileInfo = Get-Item $outputPath
    $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
    
    Write-Host "`nPackaging completed successfully!" -ForegroundColor Green
    Write-Host "Output: $outputPath" -ForegroundColor Green
    Write-Host "Size: $fileSizeMB MB" -ForegroundColor Green
    
    # Return the path to the output file for use by calling scripts or GitHub Actions
    return $outputPath
} else {
    Write-Host "Packaging failed - output file not found: $outputPath" -ForegroundColor Red
    return $null
}