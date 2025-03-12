using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

/// <summary>
/// Represents an object in a save file.
/// </summary>
public class SaveObject : SaveElement
{
    /// <summary>
    /// Gets the properties of the object.
    /// </summary>
    public ImmutableArray<KeyValuePair<string, SaveElement>> Properties { get; }

    /// <summary>
    /// Gets the type of the element.
    /// </summary>
    public override ValueType Type => ValueType.Object;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveObject"/> class.
    /// </summary>
    /// <param name="properties">The properties of the object.</param>
    public SaveObject(ImmutableArray<KeyValuePair<string, SaveElement>> properties)
    {
        Properties = properties;
    }
}