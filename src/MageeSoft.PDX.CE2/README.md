# MageeSoft.PDX.CE2

A high-performance library for parsing Paradox Clausewitz Engine save files.

## Features

- Zero-allocation parsing using `ReadOnlyMemory<char>` and `Span<char>`
- Optimized token handling with position-based token representation
- Robust error handling with detailed error messages
- Efficient date and number parsing
- Thread-safe implementation (no static state)
- Clean object model for representing save data
- Serialization support for converting back to Paradox format

## Usage Example

```csharp
using MageeSoft.PDX.CE2;

// Read a save file
string saveContent = File.ReadAllText("game_save.txt");
PdxObject saveObject = PdxSaveReader.Read(saveContent.AsMemory());

// Access data
if (saveObject.Properties.TryGetValue("player", out PdxElement playerElement) 
    && playerElement is PdxScalar<string> playerName)
{
    Console.WriteLine($"Player: {playerName.Value}");
}

// Write back to save format
var writer = new PdxSaveWriter();
string outputText = writer.Write(saveObject);
File.WriteAllText("modified_save.txt", outputText);
```

## Performance

This implementation focuses on minimizing allocations and maximizing performance:

- Uses structs instead of classes where appropriate
- Avoids unnecessary string allocations
- Provides efficient token processing
- Optimizes type inference for commonly used data types

## License

This project is licensed under the same terms as the parent project. 