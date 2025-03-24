# Versioning Guide

This project uses GitVersion for automatic semantic versioning based on Git history.

## How It Works

GitVersion analyzes your Git repository (branches, tags, commits) to determine the current semantic version number according to [SemVer 2.0](https://semver.org/) rules.

## Version Information

The version scheme follows Semantic Versioning: **MAJOR.MINOR.PATCH[-PRERELEASE]+METADATA**

- **MAJOR**: Breaking changes
- **MINOR**: New features, no breaking changes
- **PATCH**: Bugfixes, no breaking changes
- **PRERELEASE**: Optional pre-release label (e.g., alpha, beta, rc)
- **METADATA**: Build metadata, including git commit information

## File Naming Convention

All native builds use this naming convention:

```txt
paradox-clausewitz-sav_[major.minor.patch]_[platform]_[arch].[ext]
```

Where:

- **platform**: windows, linux, or macos
- **arch**: x64 or arm64
- **ext**: .zip for Windows, .tar.gz for Linux/macOS

## Controlling Version Numbers

### Using Git Commit Messages

You can specify version increments in commit messages using:

- `+semver:breaking` or `+semver:major` - Increment major version
- `+semver:feature` or `+semver:minor` - Increment minor version
- `+semver:fix` or `+semver:patch` - Increment patch version
- `+semver:none` or `+semver:skip` - Don't increment version

Example commit message:

```txt
Add new feature for handling complex save files

+semver:minor
```

### Branch Strategy

Different branches have different versioning rules:

- **main/master**: Release versions (e.g., 1.2.3)
- **develop**: Pre-release with beta tag (e.g., 1.3.0-beta.1)
- **feature/xxx**: Pre-release with alpha tag (e.g., 1.3.0-alpha.xxx.1)
- **release/xxx**: Release candidates (e.g., 1.3.0-rc.1)
- **hotfix/xxx**: Hot fixes with fix tag (e.g., 1.2.4-fix.1)

## Build Architecture

For information on how the different build scripts interact and how GitVersion is integrated into the build process, see the following documentation:

- [Build Script Architecture](etc/build-scripts.md)
- [Build Scripts Documentation](etc/README.md)

## Building with Version Information

### Native AOT Build

```powershell
./etc/build-aot.ps1 -RuntimeIdentifier win-x64
```

### .NET Tool Package

```powershell
./etc/build-tool.ps1
```

## Customizing GitVersion

For more details on configuration options, see the [GitVersion documentation](https://gitversion.net/docs/reference/configuration).
