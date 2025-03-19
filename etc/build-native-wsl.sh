#!/bin/bash
# Don't exit on errors, we want to continue building other targets
# set -e

# Define the matrix of runtime identifiers to build for using separate arrays
# This is more compatible with different bash versions
rid_array=("linux-x64" "linux-arm64")
artifact_name_array=("stellaris-sav-linux-x64" "stellaris-sav-linux-arm64")
binary_name_array=("stellaris-sav" "stellaris-sav")
source_binary_array=("StellarisSaveParser.Cli" "StellarisSaveParser.Cli")

# Function to install cross-compilation dependencies
install_dependencies() {
    echo -e "\e[36mInstalling cross-compilation dependencies...\e[0m"
        
    # https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile#linux
    sudo dpkg --add-architecture arm64    
    sudo bash -c 'cat > /etc/apt/sources.list.d/arm64.list <<EOF
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ jammy main restricted
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ jammy-updates main restricted
deb [arch=arm64] http://ports.ubuntu.com/ubuntu-ports/ jammy-backports main restricted universe multiverse
EOF'

    sudo sed -i -e 's/deb http/deb [arch=amd64] http/g' /etc/apt/sources.list
    sudo sed -i -e 's/deb mirror/deb [arch=amd64] mirror/g' /etc/apt/sources.list
    sudo apt update
    sudo apt install -y clang llvm binutils-aarch64-linux-gnu gcc-aarch64-linux-gnu zlib1g-dev:arm64

    # Install basic build tools
    sudo apt-get update
    sudo apt-get install -y clang zlib1g-dev zip
        
}

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
    
    # Build the native executable with adjusted publish arguments for linux-arm64
    publish_args=(
            -c Release
            -r "$rid"
            --self-contained true
            -p:PublishAot=true
            -p:StripSymbols=true            
            -p:OptimizationPreference=Size
            -p:PublishTrimmed=true            
            -o "$output_dir"
        )

    dotnet publish ./StellarisSaveParser.Cli/StellarisSaveParser.Cli.csproj "${publish_args[@]}"
    
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
        echo -e "\n\e[31mNative build failed for $rid with exit code $build_result\e[0m"
        return 1
    fi
}

# Always install dependencies for Linux
install_dependencies

# Build for all targets in the matrix
success_count=0
failure_count=0

# Get the length of the array
array_length=${#rid_array[@]}
echo -e "\e[36mArray length: $array_length\e[0m"
echo -e "\e[36mAvailable RIDs:\e[0m"
for rid in "${rid_array[@]}"; do
    echo -e "  $rid"
done

# Iterate over all RIDs
for i in "${!rid_array[@]}"; do
    rid="${rid_array[$i]}"
    echo -e "\e[33mProcessing RID: $rid\e[0m"
    
    artifact_name="${artifact_name_array[$i]}"
    binary_name="${binary_name_array[$i]}"
    source_binary="${source_binary_array[$i]}"
    
    echo -e "\e[33mBuilding with:\e[0m"
    echo -e "  RID: $rid"
    echo -e "  Artifact: $artifact_name"
    echo -e "  Binary: $binary_name"
    echo -e "  Source: $source_binary"
    
    if build_native_executable "$rid" "$artifact_name" "$binary_name" "$source_binary"; then
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
    exit 1
else
    echo -e "\e[32mFailed builds: $failure_count\e[0m"
    exit 0
fi