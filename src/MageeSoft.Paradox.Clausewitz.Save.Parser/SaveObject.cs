using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Parser;

/// <summary>
/// Represents an object in a save file.
/// </summary>
public class SaveObject : SaveElement
{
    public ImmutableArray<KeyValuePair<string, SaveElement>> Properties { get; }
    public override SaveType Type => SaveType.Object;

    public SaveObject(ImmutableArray<KeyValuePair<string, SaveElement>> properties)
    {
        Properties = properties;
    }
}