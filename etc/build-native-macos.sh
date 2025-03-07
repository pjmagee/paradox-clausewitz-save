#!/bin/bash
# Don't exit on errors, we want to continue building other targets
# set -e

# Set error handling
set -euo pipefail

# Define the matrix of runtime identifiers to build for using separate arrays
declare -a ridArray=("osx-x64" "osx-arm64")
declare -a artifactNameArray=("stellaris-sav-osx-x64" "stellaris-sav-osx-arm64")
declare -a binaryNameArray=("stellaris-sav-osx-x64" "stellaris-sav-osx-arm64")
declare -a sourceBinaryArray=("StellarisSaveParser.Cli" "StellarisSaveParser.Cli")

# Function to install dependencies
install_dependencies() {
    echo -e "\e[36mInstalling dependencies...\e[0m"
    
    # https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile#mac

    # Install basic build tools
    brew update
    #brew install clang zlib zip
    brew install zip
    
    # Install cross-compilation toolchain for ARM64
    # brew install llvm
    
    # Install additional dependencies for ARM64 builds
    # brew install gcc
}

# Function to build a native executable for a specific runtime identifier
build_native_executable() {
    local rid=$1
    local artifactName=$2
    local binaryName=$3
    local sourceBinary=$4
    
    echo -e "\n========================================================"
    echo -e "Building native executable for $rid..."
    echo -e "========================================================"
    
    # Create output directory
    local output_dir="./native-build"
    mkdir -p "$output_dir"
    
    # Build the native executable
    dotnet publish ./StellarisSaveParser.Cli/StellarisSaveParser.Cli.csproj \
        -c Release \
        -r $rid \
        --self-contained true \
        -p:PublishAot=true \
        -p:StripSymbols=true \
        -p:InvariantGlobalization=true \
        -p:OptimizationPreference=Size \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:EnableCompressionInSingleFile=true \
        -o "$output_dir"
    
    local build_result=$?
    
    if [ $build_result -eq 0 ]; then
        echo -e "\n\e[32mNative build completed successfully for $rid!\e[0m"
        
        local exe_path="$output_dir/$sourceBinary"
        local target_path="$output_dir/$binaryName"
        
        if [ -f "$exe_path" ]; then
            # Remove existing target file if it exists
            if [ -f "$target_path" ]; then
                rm -f "$target_path"
            fi
            
            # Rename the executable
            mv "$exe_path" "$target_path"
            echo -e "Renamed $sourceBinary to $binaryName"
            
            # Get file size
            local file_size=$(stat -f%z "$target_path")
            echo -e "Executable size: $(bc <<< "scale=2; $file_size/1024/1024") MB"
            
            return 0
        else
            echo -e "\n\e[31mExecutable not found at expected path: $exe_path\e[0m"
            echo -e "\nListing files in output directory:"
            ls -la "$output_dir"
            
            return 1
        fi
    else
        echo -e "\n\e[31mNative build failed for $rid with exit code $build_result\e[0m"
        return 1
    fi
}

# Install dependencies
install_dependencies

# Build for all targets in the matrix
success_count=0
failure_count=0

# Get the length of the array
array_length=${#ridArray[@]}
echo "Array length: $array_length"
echo "Available RIDs:"
for rid in "${ridArray[@]}"; do
    echo "  $rid"
done

# Iterate over all RIDs
for ((i=0; i<${#ridArray[@]}; i++)); do
    rid="${ridArray[$i]}"
    echo "Processing RID: $rid"
    
    artifact_name="${artifactNameArray[$i]}"
    binary_name="${binaryNameArray[$i]}"
    source_binary="${sourceBinaryArray[$i]}"
    
    echo "Building with:"
    echo "  RID: $rid"
    echo "  Artifact: $artifact_name"
    echo "  Binary: $binary_name"
    echo "  Source: $source_binary"
    
    if build_native_executable "$rid" "$artifact_name" "$binary_name" "$source_binary"; then
        ((success_count++))
    else
        ((failure_count++))
    fi
done

# Print summary
echo -e "\n========================================================"
echo -e "Build Summary"
echo -e "========================================================"
echo -e "Successful builds: $success_count"
if [ $failure_count -gt 0 ]; then
    echo -e "Failed builds: $failure_count"
    exit 1
else
    echo -e "Failed builds: $failure_count"
    exit 0
fi