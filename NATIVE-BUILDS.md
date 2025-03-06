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

- `etc/build-native.ps1` (PowerShell script for Windows)
- `etc/build-native.sh` (Bash script for Linux/macOS)

### Cross-Compilation Limitations

Native AOT does not support cross-OS compilation without emulation. This means you can only build executables for the OS you are currently running on. For more information, see the [Microsoft documentation on cross-compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/cross-compile).

### Using the PowerShell Script

By default, the PowerShell script will only build for your current OS:

```powershell
.\etc\build-native.ps1
```

To build for a specific platform only:

```powershell
.\etc\build-native.ps1 -Platform windows
```

Valid platform values are: `windows`, `linux`, `osx`.

If you want to attempt building for all platforms (not recommended due to cross-compilation limitations):

```powershell
.\etc\build-native.ps1 -ForceAll
```

### Using the Bash Script

By default, the Bash script will only build for your current OS:

```bash
chmod +x ./etc/build-native.sh
./etc/build-native.sh
```

To build for a specific platform only:

```bash
./etc/build-native.sh linux
```

Valid platform values are: `linux`, `osx`.

If you want to attempt building for all platforms (not recommended due to cross-compilation limitations):

```bash
./etc/build-native.sh --force-all
```

## Output

The native executables will be created in the `native-build` directory, organized by runtime identifier:

```
native-build/
├── win-x64/
│   ├── stellaris-sav.exe
│   └── stellaris-sav-win-x64.zip
├── win-arm64/
│   ├── stellaris-sav.exe
│   └── stellaris-sav-win-arm64.zip
├── linux-x64/
│   ├── stellaris-sav
│   └── stellaris-sav-linux-x64.zip
├── linux-arm64/
│   ├── stellaris-sav
│   └── stellaris-sav-linux-arm64.zip
├── osx-x64/
│   ├── stellaris-sav
│   └── stellaris-sav-osx-x64.zip
└── osx-arm64/
    ├── stellaris-sav
    └── stellaris-sav-osx-arm64.zip
```

## GitHub Actions Workflow

A GitHub Actions workflow is set up to automatically build native executables for all supported platforms when a new release is created. The workflow is defined in `.github/workflows/publish-native.yml`.

The workflow will:

1. Build native executables for all supported platforms (using the appropriate OS for each build)
2. Create zip files for each executable
3. Attach the zip files to the GitHub release

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

1. Download the appropriate zip file for your platform from the [Releases](https://github.com/mageesoft/stellaris-sav-parser/releases) page
2. Extract the zip file
3. Run the executable from the command line:

```bash
# Windows
stellaris-sav.exe list

# Linux/macOS
./stellaris-sav list
```

## Reflection Usage

The CLI tool uses minimal reflection, only for retrieving the assembly version in the `version` command. This doesn't impact the ability to compile to native code.

## Technical Details

The native builds use the following .NET features:

- `PublishSingleFile`: Creates a single executable file
- `PublishTrimmed`: Removes unused code to reduce the size of the executable
- `--self-contained`: Includes the .NET runtime in the executable

For the GitHub Actions workflow, we build on the appropriate OS for each target platform, which is the recommended approach for producing native executables for multiple platforms. 