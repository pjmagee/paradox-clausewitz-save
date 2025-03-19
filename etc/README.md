# Build Scripts

This directory contains scripts for building and publishing the Stellaris Save Parser.

## Native AOT Builds

### Windows

```powershell
# Build a Native AOT executable for Windows
./build-aot.ps1 -RuntimeIdentifier win-x64

# Build for other platforms
./build-aot.ps1 -RuntimeIdentifier linux-x64
./build-aot.ps1 -RuntimeIdentifier osx-x64

# Specify configuration
./build-aot.ps1 -RuntimeIdentifier win-x64 -Configuration Debug
```

## .NET Tool Package

```powershell
# Build a .NET Tool package
./build-tool.ps1

# Specify configuration
./build-tool.ps1 -Configuration Debug
```

## Using GitVersion

The build scripts automatically use GitVersion to determine version numbers from Git history.

See the versioning.md file in the repo root for details on the versioning scheme and how to control version numbers with Git branches and commit messages.

## Other Scripts

- **build-native-windows.ps1** - Alternative script for building native binaries on Windows
- **build-native-wsl.sh** - Script for building native binaries using WSL
- **build-native-macos.sh** - Script for building native binaries on macOS
- **publish-nuget.ps1** - Script for publishing NuGet packages
- **test-tool.ps1** - Script for testing the tool
- **Dockerfile.linux-cross** - Dockerfile for cross-compilation to Linux 