using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

/// <summary>
/// Represents an array in a save file.
/// </summary>
public class SaveArray : SaveElement
{
    /// <summary>
    /// Gets the items in the array.
    /// </summary>
    public ImmutableArray<SaveElement> Items { get; }

    /// <summary>
    /// Gets the type of the element.
    /// </summary>
    public override SaveType Type => SaveType.Array;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveArray"/> class.
    /// </summary>
    /// <param name="items">The items in the array.</param>
    public SaveArray(ImmutableArray<SaveElement> items)
    {
        Items = items;
    }
}