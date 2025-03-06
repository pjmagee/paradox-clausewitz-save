#!/bin/bash
set -e

# Define the matrix of runtime identifiers to build for
declare -A matrix
matrix[0,rid]="linux-x64"
matrix[0,artifact_name]="stellaris-sav-linux-x64"
matrix[0,binary_name]="stellaris-sav"
matrix[0,source_binary]="StellarisSaveParser.Cli"

matrix[1,rid]="linux-arm64"
matrix[1,artifact_name]="stellaris-sav-linux-arm64"
matrix[1,binary_name]="stellaris-sav"
matrix[1,source_binary]="StellarisSaveParser.Cli"

matrix[2,rid]="osx-x64"
matrix[2,artifact_name]="stellaris-sav-osx-x64"
matrix[2,binary_name]="stellaris-sav"
matrix[2,source_binary]="StellarisSaveParser.Cli"

matrix[3,rid]="osx-arm64"
matrix[3,artifact_name]="stellaris-sav-osx-arm64"
matrix[3,binary_name]="stellaris-sav"
matrix[3,source_binary]="StellarisSaveParser.Cli"

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
    local output_dir="../native-build/$rid"
    mkdir -p "$output_dir"
    
    # Build the native executable
    dotnet publish ../StellarisSaveParser.Cli/StellarisSaveParser.Cli.csproj \
        -c Release \
        -r $rid \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
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
    current_os="osx"
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
force_all=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --force-all|-f)
            force_all=true
            shift
            ;;
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
elif [ "$force_all" = true ]; then
    echo -e "\e[31mWARNING: Forcing build for ALL platforms regardless of current OS\e[0m"
    echo -e "\e[31mThis may fail due to cross-compilation limitations. See: https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile\e[0m"
else
    echo -e "\e[33mBuilding only for current OS: $current_os\e[0m"
    echo -e "\e[33mUse --force-all or -f to attempt building for all platforms (may fail)\e[0m"
    
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
    rid="${matrix[$i,rid]}"
    
    # Skip if not matching target platform
    if [ -n "$target_platform" ] && [[ "$rid" != $target_platform* ]]; then
        echo -e "\nSkipping $rid (not matching target platform $target_platform)"
        continue
    fi
    
    artifact_name="${matrix[$i,artifact_name]}"
    binary_name="${matrix[$i,binary_name]}"
    source_binary="${matrix[$i,source_binary]}"
    
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
echo -e "\n\e[36mNative executables are available in the ../native-build directory\e[0m"

# Return success if all builds succeeded
if [ $failure_count -gt 0 ]; then
    exit 1
else
    exit 0
fi 