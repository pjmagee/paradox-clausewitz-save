namespace MageeSoft.PDX.CE;

/// <summary>
/// Token types for the Paradox save parser.
/// </summary>
public enum PdxTokenType
{
    Identifier,   // Unquoted strings, keywords like yes/no
    NumberLiteral, // Numeric values
    StringLiteral, // Quoted string values
    CurlyOpen,    // { character
    CurlyClose,   // } character
    Equals,       // = character
    Whitespace,   // Space, tab
    NewLine,      // CR, LF, CRLF
    EndOfFile,    // End of input
    Unknown       // Unexpected character
}