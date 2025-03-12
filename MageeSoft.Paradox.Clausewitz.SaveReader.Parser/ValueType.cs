namespace MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

public enum ValueType
{
    /// <summary>
    /// A save object.
    /// </summary>
    Object,

    /// <summary>
    /// A save array.
    /// </summary>
    Array,

    /// <summary>
    /// A scalar string literal.
    /// </summary>
    String,
    /// <summary>
    /// A scalar identifier.
    /// </summary>
    Identifier,
    /// <summary>
    /// A scalar boolean literal.
    /// </summary>
    Boolean,
    /// <summary>
    /// A scalar float literal.
    /// </summary>
    Float,
    /// <summary>
    /// A scalar 32-bit integer literal.
    /// </summary>
    Int32,
    /// <summary>
    /// A scalar 4-bit integer literal.
    /// </summary>
    Int64,
    /// <summary> 
    /// A scalar date literal.
    /// </summary>
    Date,
    /// <summary>
    /// A GUID literal.
    /// </summary>
    Guid
}