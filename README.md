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

## Build System and Versioning

This project uses a comprehensive build system to create native executables and packages for multiple platforms:

- [Versioning Guide](versioning.md) - How version numbers are determined using GitVersion
- [Build Script Architecture](etc/build-scripts.md) - How the build scripts work together
- [Build Scripts Documentation](etc/README.md) - Reference for all build scripts

The build system produces native executables for:

- Windows (x64, arm64)
- Linux (x64, arm64)
- macOS (x64, arm64)

All builds use a consistent naming convention: `paradox-clausewitz-sav_[version]_[platform]_[arch].[ext]`
