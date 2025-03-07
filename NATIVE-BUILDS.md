# Native Builds for Stellaris Save Parser CLI

This document describes the process of building native executables for the Stellaris Save Parser CLI tool.

## Overview

The Stellaris Save Parser CLI tool can be built as native executables for various platforms using .NET's Native AOT compilation. This allows users to run the tool without having .NET installed on their system.

## Supported Platforms

The following platforms are supported:

- Windows (x64, ARM64)
- Linux (x64, ARM64)
- macOS (x64, ARM64)

## Building Native Executables

There are two scripts provided to build native executables:

- `etc/build-native-windows.ps1` (PowerShell script for Windows)
- `etc/build-native-macos.sh` (Bash script for macOS)

### Cross-Compilation Limitations

Native AOT does not support cross-OS compilation without emulation. This means you can only build executables for the OS you are currently running on. For more information, see the [Microsoft documentation on cross-compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile).

### Using the PowerShell Script

By default, the PowerShell script will only build for your current OS:

```powershell
.\etc\build-native-windows.ps1
```

### Using the Bash Script

By default, the Bash script will only build for your current OS:

```bash
chmod +x ./etc/build-native-macos.sh
./etc/build-native-macos.sh
```

## Output

The native executables will be created in the `native-build` directory:

```
native-build/
├── stellaris-sav-win-x64.exe
├── stellaris-sav-win-arm64.exe
├── stellaris-sav-linux-x64
├── stellaris-sav-linux-arm64
├── stellaris-sav-osx-x64
└── stellaris-sav-osx-arm64
```

## GitHub Actions Workflow

A GitHub Actions workflow is set up to automatically build native executables for all supported platforms when a new release is created. The workflow is defined in `.github/workflows/publish-native.yml`.

The workflow will:
1. Build native executables for all supported platforms (using the appropriate OS for each build)
2. Upload the executables directly to the GitHub release

## Requirements for Building

To build native executables, you need:

- .NET SDK 8.0 or later
- For cross-architecture builds (e.g., x64 to ARM64 on the same OS), the appropriate .NET workloads installed:
  - `dotnet workload install wasm-tools`
  - `dotnet workload install android`
  - `dotnet workload install ios`
  - `dotnet workload install macos`
  - `dotnet workload install tvos`

Note that some platforms may require additional dependencies for cross-compilation.

## Using Native Executables

1. Download the appropriate executable for your platform from the [Releases](https://github.com/mageesoft/stellaris-sav-parser/releases) page
2. Make the file executable (Linux/macOS only):
   ```bash
   chmod +x stellaris-sav-linux-x64  # or appropriate filename
   ```
3. Run the executable from the command line:

```bash
# Windows
stellaris-sav-win-x64.exe list

# Linux/macOS
./stellaris-sav-linux-x64 list
```

## Reflection Usage

The CLI tool uses minimal reflection, only for retrieving the assembly version in the `version` command. This doesn't impact the ability to compile to native code.

## Technical Details

The native builds use the following .NET features:

- `PublishSingleFile`: Creates a single executable file
- `PublishTrimmed`: Removes unused code to reduce the size of the executable
- `--self-contained`: Includes the .NET runtime in the executable

For the GitHub Actions workflow, we build on the appropriate OS for each target platform, which is the recommended approach for producing native executables for multiple platforms. 