#!/bin/bash
set -e

# Check required parameters
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
    echo "Usage: $0 <input-file> <platform> <architecture>"
    echo "  <platform>: linux|macos"
    echo "  <architecture>: x64|arm64"
    exit 1
fi

# Input parameters
INPUT_FILE="$1"
PLATFORM="$2"
ARCHITECTURE="$3"
OUTPUT_DIR="${4:-./artifacts}"

# Validate platform
if [ "$PLATFORM" != "linux" ] && [ "$PLATFORM" != "macos" ]; then
    echo "Error: Platform must be linux or macos"
    exit 1
fi

# Validate architecture
if [ "$ARCHITECTURE" != "x64" ] && [ "$ARCHITECTURE" != "arm64" ]; then
    echo "Error: Architecture must be x64 or arm64"
    exit 1
fi

# Ensure output directory exists
mkdir -p "$OUTPUT_DIR"

# Get version info from GitVersion
echo "Getting version information from GitVersion..."
VERSION_INFO=$(dotnet gitversion /output json)
VERSION_STRING=$(echo "$VERSION_INFO" | grep -o '"MajorMinorPatch": *"[^"]*"' | cut -d '"' -f 4)
SEMVER=$(echo "$VERSION_INFO" | grep -o '"SemVer": *"[^"]*"' | cut -d '"' -f 4)
SHA=$(echo "$VERSION_INFO" | grep -o '"Sha": *"[^"]*"' | cut -d '"' -f 4)
BRANCH=$(echo "$VERSION_INFO" | grep -o '"BranchName": *"[^"]*"' | cut -d '"' -f 4)

echo "Version: $SEMVER"
echo "Commit: $SHA"
echo "Branch: $BRANCH"

# Set up file paths and names
BASE_FILENAME="paradox-clausewitz-sav_${VERSION_STRING}_${PLATFORM}_${ARCHITECTURE}"
OUTPUT_FILENAME="${BASE_FILENAME}.tar.gz"
OUTPUT_PATH="${OUTPUT_DIR}/${OUTPUT_FILENAME}"

# Create temporary directory for packaging
TEMP_DIR=$(mktemp -d)
TARGET_FILENAME="paradox-clausewitz-sav"
TARGET_PATH="${TEMP_DIR}/${TARGET_FILENAME}"

# Copy binary to temp dir
cp "$INPUT_FILE" "$TARGET_PATH"
chmod +x "$TARGET_PATH"

# Create version.txt file
cat > "${TEMP_DIR}/version.txt" << EOF
Version: $SEMVER
Commit: $SHA
Branch: $BRANCH
Built at: $(date "+%Y-%m-%d %H:%M:%S")
EOF

# Package the files
echo "Packaging ${TARGET_FILENAME} to ${OUTPUT_PATH}..."
tar -czf "$OUTPUT_PATH" -C "$TEMP_DIR" .

# Clean up
rm -rf "$TEMP_DIR"

# Output success message
if [ -f "$OUTPUT_PATH" ]; then
    FILE_SIZE=$(du -h "$OUTPUT_PATH" | cut -f1)
    echo "Packaging completed successfully!"
    echo "Output: $OUTPUT_PATH"
    echo "Size: $FILE_SIZE"
    echo "$OUTPUT_PATH"
else
    echo "Packaging failed - output file not found: $OUTPUT_PATH"
    exit 1
fi 