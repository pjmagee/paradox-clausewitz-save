# Paradox Clausewitz Save Reader

This library is a parser for the Proprietary Clausewitz Engine save file format used by Paradox Interactive games.
It allows you to read and the contents of these save files in a structured way.
Inspired by `System.Text.Json`, this library provides a simple API to read the data in the save files.

## Key Features

- **NuGet Package Publishing**: Available as packages through NuGet
- **Source Generator for Model Binding**: Automatically generate binding code for your models
- **Native AOT Compilation**: Full support for Native AOT compilation across Windows, Linux, and macOS on both x64 and arm64 architectures, producing fully native binaries that require absolutely no dependency on the .NET Runtime
- **System.Text.Json Inspired API**: Familiar and intuitive API design following patterns from System.Text.Json

## Supported Games

| Game | Status |
|------|--------|
| Stellaris | ✅ Supported |
| Hearts of Iron IV | ⏳ Pending |
| Europa Universalis IV | ⏳ Pending |
| Crusader Kings III | ⏳ Pending |
| Victoria III | ⏳ Pending |

## Usage Examples

### NuGet Package Usage

```csharp
// Load a Stellaris save file
StellarisSave save = StellarisSave.FromSave("path/to/savegame.sav");

// Access meta information
Console.WriteLine($"Save Name: {save.Meta.Name}");
Console.WriteLine($"Game Version: {save.Meta.Version}");
Console.WriteLine($"Game Date: {save.Meta.Date}"); // DateOnly format

// Access game state data
Console.WriteLine($"Empire Name: {save.GameState.Player.EmpireName}");
// Access other game state properties as needed
```

### CLI Commands

The library includes a command-line tool (`paradox-clausewitz-sav`) with the following commands:

```bash
# List all available save files for a specific game
paradox-clausewitz-sav list --game stellaris

# Show a summary of a save file (using index from list command)
paradox-clausewitz-sav summary --game stellaris --number 1 [--format text|json] [--output file.txt]

# Export a save file to JSON format
paradox-clausewitz-sav json --game stellaris --number 1 [--output save.json]

# Display version information about the tool
paradox-clausewitz-sav info
```

The CLI tool is available in two formats:

1. As a native AOT binary that requires no .NET runtime dependency
2. As a .NET global tool that can be installed via the dotnet CLI

#### Installing as a .NET Global Tool

```bash
# Install the tool globally on your system
dotnet tool install --global MageeSoft.Paradox.Clausewitz.Save.Cli

# Update to the latest version
dotnet tool update --global MageeSoft.Paradox.Clausewitz.Save.Cli
```

## PowerShell Scripts and Configuration

The `/etc` directory contains configuration files and utility scripts used for development, building, and maintaining the project. This follows the Unix-like convention of storing system-wide configuration files in `/etc`.

## Why PowerShell?

PowerShell is used across all platforms (Windows, Linux, macOS) in this project because:

1. It provides true cross-platform support through PowerShell Core (Pwsh)
2. Maintains a single codebase for scripts rather than maintaining separate bash and PowerShell versions
3. Offers consistent behavior across different operating systems
4. Provides rich object manipulation and .NET integration
5. Reduces maintenance overhead by avoiding platform-specific script variants

All scripts in this directory are written in PowerShell and can be executed using PowerShell Core (pwsh) on any supported platform.

## Versioning

This project uses a comprehensive build system to create native executables and packages for multiple platforms:

- [Versioning Guide](versioning.md) - How version numbers are determined using GitVersion

The build system produces native executables for:

- Windows (x64, arm64)
- Linux (x64, arm64)
- macOS (x64, arm64)

All builds use a consistent naming convention: `paradox-clausewitz-sav_[version]_[platform]_[arch].[ext]`
