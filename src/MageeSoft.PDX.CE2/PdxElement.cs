namespace MageeSoft.PDX.CE2;

/// <summary>
/// Base class for all save file elements (V2).
/// </summary>
public abstract class PdxElement
{
    /// <summary>
    /// Gets the type of the save element.
    /// </summary>
    /// <remarks>
    /// This is used primarily for serialization and for differentiating element types
    /// without using reflection (better performance).
    /// </remarks>
    public abstract PdxType Type { get; }

    /// <summary>
    /// Converts the element to its string representation in the Paradox save format.
    /// </summary>
    /// <returns>A string representation of the element.</returns>
    public override string ToString()
    {
        var writer = new PdxSaveWriter();
        return writer.Write(this);
    }
} 