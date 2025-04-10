namespace MageeSoft.PDX.CE2;

/// <summary>
/// Token types specifically for the PdxSaveReader.
/// </summary>
internal enum PdxTokenType
{
    Identifier, // Includes unquoted strings, keywords like yes/no
    NumberLiteral,
    StringLiteral, // Quoted string
    CurlyOpen,
    CurlyClose,
    Equals,
    Whitespace,
    NewLine,
    EndOfFile,
    Unknown // For error handling or unexpected characters
}