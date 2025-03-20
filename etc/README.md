# Build Scripts

This directory contains scripts for building and publishing the Stellaris Save Parser.

## Primary Build Scripts (Used by GitHub Actions)

| Script | Description | Used By |
|--------|-------------|---------|
| `build-native-windows.ps1` | Builds native executables for Windows (x64/arm64) | `publish-native.yml` |
| `build-native-macos.sh` | Builds native executables for macOS (x64/arm64) | `publish-native.yml` |
| `Dockerfile.linux-cross` | Docker image for cross-compiling Linux binaries | `publish-native.yml` |
| `package-native.ps1` | Packages Windows builds with version info | `build-native-windows.ps1` |
| `package-native.sh` | Packages macOS/Linux builds with version info | `build-native-macos.sh` |

## Tool Package Scripts

| Script | Description |
|--------|-------------|
| `build-tool.ps1` | Builds a .NET global tool package |
| `publish-nuget.ps1` | Publishes packages to NuGet |

## Legacy/Local Development Scripts

The following scripts are kept for local development but are NOT used in the CI pipeline:

| Script | Description |
|--------|-------------|
| `build-aot.ps1` | Simple AOT build script for local testing |
| `build-native-wsl.sh` | Alternative build script using WSL |
| `test-tool.ps1` | Script for testing the tool |

## Using the Scripts

### Building Native Executables

```powershell
# Windows
./build-native-windows.ps1  # Builds for x64 and arm64

# macOS (on macOS)
chmod +x ./build-native-macos.sh
./build-native-macos.sh  # Builds for x64 and arm64

# Linux (using Docker)
docker build -f Dockerfile.linux-cross --build-arg TARGETARCH=x64 -t linux-build .
```

### Building .NET Tool Package

```powershell
./build-tool.ps1
```

## Versioning

All builds use GitVersion to automatically determine version numbers. This creates consistent versioning across all platforms with filenames in this format:

```
paradox-clausewitz-sav_[major.minor.patch]_[platform]_[arch].[ext]
```

For more details on the versioning approach, see the versioning.md file in the repo root.
