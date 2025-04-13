namespace MageeSoft.PDX.CE;

/// <summary>
/// Interface for all Paradox save elements (V2).
/// </summary>
public interface IPdxElement
{
    /// <summary>
    /// Gets the element type.
    /// </summary>
    PdxType Type { get; }
} 