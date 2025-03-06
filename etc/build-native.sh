#!/bin/bash
set -e

# Define the matrix of runtime identifiers to build for using separate arrays
# This is more compatible with different bash versions
rid_array=("linux-x64" "linux-arm64" "osx-x64" "osx-arm64")
artifact_name_array=("stellaris-sav-linux-x64" "stellaris-sav-linux-arm64" "stellaris-sav-osx-x64" "stellaris-sav-osx-arm64")
binary_name_array=("stellaris-sav" "stellaris-sav" "stellaris-sav" "stellaris-sav")
source_binary_array=("StellarisSaveParser.Cli" "StellarisSaveParser.Cli" "StellarisSaveParser.Cli" "StellarisSaveParser.Cli")

# Function to build a native executable for a specific runtime identifier
build_native_executable() {
    local rid=$1
    local artifact_name=$2
    local binary_name=$3
    local source_binary=$4
    
    echo -e "\n========================================================"
    echo -e "\e[36mBuilding native executable for $rid...\e[0m"
    echo -e "========================================================"
    
    # Create output directory
    local output_dir="./native-build/$rid"
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
        
        local exe_path="$output_dir/$source_binary"
        
        if [ -f "$exe_path" ]; then
            # Rename the executable
            local target_path="$output_dir/$binary_name"
            mv "$exe_path" "$target_path"
            echo -e "\e[32mRenamed $source_binary to $binary_name\e[0m"
            
            # Make executable
            chmod +x "$target_path"
            
            # Get file size
            local file_size=$(du -h "$target_path" | cut -f1)
            echo -e "\e[36mExecutable size: $file_size\e[0m"
            
            # Create zip file
            local zip_path="$output_dir/$artifact_name.zip"
            (cd "$output_dir" && zip "$artifact_name.zip" "$binary_name")
            echo -e "\e[32mCreated zip file: $zip_path\e[0m"
            
            return 0
        else
            echo -e "\n\e[31mExecutable not found at expected path: $exe_path\e[0m"
            echo -e "\n\e[33mListing files in output directory:\e[0m"
            ls -la "$output_dir"
            
            return 1
        fi
    else
        echo -e "\n\e[31mNative build failed for $rid\e[0m"
        return 1
    fi
}

# Determine current OS and architecture
current_os="unknown"
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    current_os="linux"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    current_os="macos"
else
    echo -e "\e[31mUnsupported OS: $OSTYPE\e[0m"
    exit 1
fi

# Check for ARM architecture
is_arm=false
if [[ $(uname -m) == "arm64" ]] || [[ $(uname -m) == "aarch64" ]]; then
    is_arm=true
fi

echo -e "\e[36mDetected OS: $current_os (ARM: $is_arm)\e[0m"

# Parse command line arguments
target_platform=""

while [[ $# -gt 0 ]]; do
    case $1 in
        *)
            # Assume it's a platform specification
            target_platform=$1
            shift
            ;;
    esac
done

# Determine build strategy
if [ -n "$target_platform" ]; then
    echo -e "\e[33mBuilding only for platform: $target_platform\e[0m"
else
    echo -e "\e[33mBuilding only for current OS: $current_os\e[0m"
    
    # Map OS name to RID prefix (no change needed for Linux)
    if [ "$current_os" = "macos" ]; then
        target_platform="osx"
    else
        target_platform=$current_os
    fi
fi

# Build for all targets in the matrix
success_count=0
failure_count=0

for i in {0..3}; do
    rid="${rid_array[$i]}"
    
    # Skip if not matching target platform
    if [ -n "$target_platform" ] && [[ "$rid" != $target_platform* ]]; then
        echo -e "\nSkipping $rid (not matching target platform $target_platform)"
        continue
    fi
    
    artifact_name="${artifact_name_array[$i]}"
    binary_name="${binary_name_array[$i]}"
    source_binary="${source_binary_array[$i]}"
    
    build_native_executable "$rid" "$artifact_name" "$binary_name" "$source_binary"
    
    if [ $? -eq 0 ]; then
        ((success_count++))
    else
        ((failure_count++))
    fi
done

# Print summary
echo -e "\n========================================================"
echo -e "\e[36mBuild Summary\e[0m"
echo -e "========================================================"
echo -e "\e[32mSuccessful builds: $success_count\e[0m"
if [ $failure_count -gt 0 ]; then
    echo -e "\e[31mFailed builds: $failure_count\e[0m"
else
    echo -e "\e[32mFailed builds: $failure_count\e[0m"
fi
echo -e "\n\e[36mNative executables are available in the ./native-build directory\e[0m"

# Return success if all builds succeeded
if [ $failure_count -gt 0 ]; then
    exit 1
else
    exit 0
fi 