# Stellaris Save Parser

[![.NET Build and Test](https://github.com/pjmagee/stellaris-sav-parser/actions/workflows/dotnet.yml/badge.svg)](https://github.com/pjmagee/stellaris-sav-parser/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/pjmagee/stellaris-sav-parser/branch/main/graph/badge.svg)](https://codecov.io/gh/pjmagee/stellaris-sav-parser)

A .NET library for parsing Stellaris save files (.sav), providing easy access to both gamestate and meta information. The library supports both regular and Ironman save files.

The library is designed to be used in a similar manner to JsonDocument, with a focus on readability and ease of use.

## Features

- Parse Stellaris .sav files
- Access both gamestate and meta information
- JsonDocument-like API for easy querying

## Installation

```bash
dotnet add package MageeSoft.StellarisSaveParser --version 0.0.1
```

## Quick Start

```csharp
using StellarisSaveParser;

// Load and parse a save file
var saveFile = new FileInfo("mysave.sav");
var documents = GameSaveZip.Unzip(saveFile);

// Access meta information
var version = documents.Meta.RootElement
    .EnumerateObject()
    .First(p => p.Key == "version")
    .Value.ToString();

Console.WriteLine($"Game Version: {version}");

// Access basic game information
var date = documents.Meta.RootElement
    .EnumerateObject()
    .First(p => p.Key == "date")
    .Value.ToString();

var empireName = documents.Meta.RootElement
    .EnumerateObject()
    .First(p => p.Key == "name")
    .Value.ToString();

Console.WriteLine($"Empire: {empireName}");
Console.WriteLine($"Date: {date}");
```

## Common Usage Examples

### Accessing Empire Information

```csharp
// Get player empire details
var player = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "player")
    .Value;

// Get empire flag
var flag = documents.Meta.RootElement
    .EnumerateObject()
    .First(p => p.Key == "flag")
    .Value;

// Get empire's fleet count
var fleetCount = documents.Meta.RootElement
    .EnumerateObject()
    .First(p => p.Key == "meta_fleets")
    .Value.ToString();
```

### Accessing Galaxy Information

```csharp
// Get galaxy information
var galaxy = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "galaxy")
    .Value;

// Get all planets
var planets = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "planets")
    .Value;

// Get all fleets
var fleets = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "fleet")
    .Value;
```

### Accessing Species Information

```csharp
// Get species database
var speciesDb = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "species_db")
    .Value;

// Get all pops
var pops = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "pop")
    .Value;
```

### Accessing Market and Economy

```csharp
// Get market information
var market = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "market")
    .Value;

// Get trade routes
var tradeRoutes = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "trade_routes")
    .Value;
```

### Accessing Diplomatic Information

```csharp
// Get federations
var federations = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "federation")
    .Value;

// Get wars
var wars = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "war")
    .Value;

// Get agreements
var agreements = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "agreements")
    .Value;
```

### Accessing Military Information

```csharp
// Get all ships
var ships = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "ships")
    .Value;

// Get all armies
var armies = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "army")
    .Value;

// Get ship designs
var shipDesigns = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "ship_design")
    .Value;
```

## Advanced Usage

### Handling Arrays

```csharp
// Example: Reading an array of values
var values = element.EnumerateArray().ToList();
foreach (var value in values)
{
    Console.WriteLine(value.ToString());
}
```

### Handling Objects

```csharp
// Example: Reading object properties
var properties = element.EnumerateObject().ToList();
foreach (var prop in properties)
{
    Console.WriteLine($"{prop.Key}: {prop.Value}");
}
```

### Handling Nested Structures

```csharp
// Example: Navigating nested structures
var countryName = documents.GameState.RootElement
    .EnumerateObject()
    .First(p => p.Key == "country")
    .Value
    .EnumerateObject()
    .First(p => p.Key == "name")
    .Value
    .EnumerateObject()
    .First(p => p.Key == "key")
    .Value;
```

## Error Handling

The library throws appropriate exceptions for common error cases:

```csharp
try
{
    var documents = GameSaveZip.Unzip(saveFile);
}
catch (FileNotFoundException)
{
    // Handle missing save file
}
catch (InvalidDataException)
{
    // Handle invalid or corrupted save file
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## CLI Tool

The Stellaris Save Parser is also available as a .NET global tool. You can install it using the following command:

```bash
dotnet tool install --global MageeSoft.StellarisSaveParser.Cli
```

### CLI Commands

Once installed, you can use the tool with the following commands:

```bash
# List all Stellaris save files on your system
stellaris-sav list

# Show a numbered list for easy reference
stellaris-sav list --numbered

# Show full paths instead of shortened paths
stellaris-sav list --full-path

# Sort save files by name, date, or size
stellaris-sav list --sort name
stellaris-sav list --sort date
stellaris-sav list --sort size

# Summarize a save file by path
stellaris-sav summarize path/to/save/file.sav

# Summarize a save file by number from the list
stellaris-sav summarize --number 1
```