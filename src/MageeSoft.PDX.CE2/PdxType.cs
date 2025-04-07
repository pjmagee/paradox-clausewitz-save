namespace MageeSoft.PDX.CE2;

/// <summary>
/// Represents the type of a save element (V2).
/// </summary>
/// <remarks>
/// This enum provides a more efficient way to identify element types without using
/// runtime type checking or reflection, while also providing context on the specific
/// scalar type for optimized processing.
/// </remarks>
public enum PdxType
{
    /// <summary>
    /// A key-value object containing properties.
    /// </summary>
    Object,
    
    /// <summary>
    /// An array of values.
    /// </summary>
    Array,
    
    /// <summary>
    /// A quoted string value.
    /// </summary>
    String,
    
    /// <summary>
    /// An unquoted string (identifier).
    /// </summary>
    Identifier,
    
    /// <summary>
    /// A boolean value (represented as "yes" or "no").
    /// </summary>
    Bool,
    
    /// <summary>
    /// A 32-bit integer value.
    /// </summary>
    Int32,
    
    /// <summary>
    /// A 64-bit integer value.
    /// </summary>
    Int64,
    
    /// <summary>
    /// A floating-point value.
    /// </summary>
    Float,
    
    /// <summary>
    /// A date value (in Paradox format like "yyyy.mm.dd").
    /// </summary>
    Date,
    
    /// <summary>
    /// A GUID value.
    /// </summary>
    Guid
} 