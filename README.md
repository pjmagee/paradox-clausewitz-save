# Paradox Clausewitz Save Reader

This library is a parser for the Proprietary Clausewitz Engine save file format used by Paradox Interactive games.
It allows you to read and the contents of these save files in a structured way.
Inspired by `System.Text.Json`, this library provides a simple API to read the data in the save files.

## Supported Games

- Stellaris
- Hearts of Iron IV
- Europa Universalis IV
- Crusader Kings III
- Victoria III

## Versioning

This project uses a comprehensive build system to create native executables and packages for multiple platforms:

- [Versioning Guide](versioning.md) - How version numbers are determined using GitVersion

The build system produces native executables for:

- Windows (x64, arm64)
- Linux (x64, arm64)
- macOS (x64, arm64)

All builds use a consistent naming convention: `paradox-clausewitz-sav_[version]_[platform]_[arch].[ext]`

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
