namespace MageeSoft.PDX.CE2;

/// <summary>
/// Represents the type of a Paradox save element (V2).
/// </summary>
public enum PdxType
{
    Object,
    Array,
    String,
    Bool,
    Int32,
    Int64,
    Float,
    Date,
    Guid
}

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