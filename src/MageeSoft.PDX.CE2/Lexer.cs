using System.Text; // Keep for string literal processing

namespace MageeSoft.PDX.CE2;

/// <summary>
/// Represents a token identified by the Lexer (V2).
/// Stores the token type, start position, and length within the input memory.
/// Optionally stores a processed string (e.g., for unescaped string literals).
/// </summary>
public readonly struct Token // Changed from record struct
{
    public TokenType Type { get; }
    public int Start { get; }
    public int Length { get; }
    public string? ProcessedString { get; }

    public Token(TokenType type, int start, int length, string? processedString = null)
    {
        Type = type;
        Start = start;
        Length = length;
        ProcessedString = processedString;
    }

    // Optional: Add Equals/GetHashCode if needed for Token comparisons
}

/// <summary>
/// Token types for the Lexer (V2).
/// </summary>
public enum TokenType
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

/// <summary>
/// Lexer implementation using ReadOnlyMemory/Span (V2, performance focused).
/// No longer a ref struct.
/// </summary>
public struct Lexer // Removed 'ref'
{
    private readonly ReadOnlyMemory<char> _inputMemory; // Use Memory for potential slicing
    private int _position;

    public Lexer(ReadOnlyMemory<char> input)
    {
        _inputMemory = input;
        _position = 0;
    }

    // Public property to get current position
    public int CurrentPosition => _position;

    // Public method to set position
    public void SetPosition(int position)
    {
        _position = position >= 0 && position <= _inputMemory.Length ? position : throw new ArgumentOutOfRangeException(nameof(position));
    }

    private ReadOnlySpan<char> InputSpan => _inputMemory.Span;
    private char CurrentCharOrDefault => _position < _inputMemory.Length ? _inputMemory.Span[_position] : '\0';
    private char PeekCharOrDefault => (_position + 1) < _inputMemory.Length ? _inputMemory.Span[_position + 1] : '\0';

    private void Advance() => _position++;

    public Token NextToken()
    {
        if (_position >= _inputMemory.Length)
            return new Token(TokenType.EndOfFile, _position, 0);

        int start = _position;
        char current = CurrentCharOrDefault;

        // --- Whitespace and Newlines ---
        if (current == '\n' || current == '\r')
        {
            Advance(); // Consume \n or \r
            if (current == '\r' && CurrentCharOrDefault == '\n') Advance(); // Consume \n for \r\n
            return new Token(TokenType.NewLine, start, _position - start);
        }
        if (char.IsWhiteSpace(current))
        {
            while (_position < _inputMemory.Length && char.IsWhiteSpace(CurrentCharOrDefault) && CurrentCharOrDefault != '\n' && CurrentCharOrDefault != '\r') Advance();
            return new Token(TokenType.Whitespace, start, _position - start);
        }

        // --- Single Character Tokens ---
        if (current == '{') { Advance(); return new Token(TokenType.CurlyOpen, start, 1); }
        if (current == '}') { Advance(); return new Token(TokenType.CurlyClose, start, 1); }
        if (current == '=') { Advance(); return new Token(TokenType.Equals, start, 1); }

        // --- String Literal ---
        if (current == '"')
        {
            Advance(); // Skip opening quote
            StringBuilder? sb = null;
            int contentStart = _position;

            while (_position < _inputMemory.Length)
            {
                current = CurrentCharOrDefault;
                if (current == '"') break; // End of string
                if (current == '\\')
                {
                    if (PeekCharOrDefault == '"')
                    {
                        sb ??= new StringBuilder();
                        sb.Append(InputSpan.Slice(contentStart, _position - contentStart).ToString()); // Append segment before escape
                        Advance(); // Skip '\\'
                        sb.Append('"');
                        Advance(); // Skip escaped '"'
                        contentStart = _position;
                        continue; // Continue loop
                    }
                    // Handle other escapes if needed (e.g., \n, \t, \\) - currently treats \ followed by non-" as literal
                }
                Advance();
            }

            int endQuotePos = _position; // Position of closing quote or end of input
            if (CurrentCharOrDefault == '"') Advance(); // Consume closing quote if present

            string? processedString;
            if (sb != null) // Escapes were processed
            {
                if (endQuotePos > contentStart) sb.Append(InputSpan.Slice(contentStart, endQuotePos - contentStart).ToString()); // Append final segment
                processedString = sb.ToString();
            }
            else // No escapes
            {
                // Create string from the span between quotes
                int contentLen = Math.Max(0, endQuotePos - (start + 1));
                processedString = contentLen > 0 ? InputSpan.Slice(start + 1, contentLen).ToString() : string.Empty;
            }
            return new Token(TokenType.StringLiteral, start, _position - start, processedString);
        }

        // --- Number Literal ---
        bool isPotentiallyNumber = char.IsDigit(current) ||
                                   (current == '-' && (char.IsDigit(PeekCharOrDefault) || PeekCharOrDefault == '.')) ||
                                   (current == '.' && char.IsDigit(PeekCharOrDefault));
        if (isPotentiallyNumber)
        {
            int numStart = _position;
            bool hasDecimal = (current == '.');
            bool hasDigit = char.IsDigit(current);
            if (current == '-') Advance();

            while (_position < _inputMemory.Length)
            {
                current = CurrentCharOrDefault;
                if (char.IsDigit(current)) { hasDigit = true; Advance(); }
                else if (current == '.' && !hasDecimal && char.IsDigit(PeekCharOrDefault)) { hasDecimal = true; Advance(); }
                else break;
            }

            if (hasDigit) return new Token(TokenType.NumberLiteral, numStart, _position - numStart);
            else _position = numStart; // Roll back if not a valid number
        }

        // --- Identifier ---
        if (IsIdentifierStartChar(current))
        {
            int idStart = _position;
            Advance();
            while (_position < _inputMemory.Length && IsIdentifierChar(CurrentCharOrDefault)) Advance();
            return new Token(TokenType.Identifier, idStart, _position - idStart);
        }

        // --- Unknown Token ---
        Advance();
        return new Token(TokenType.Unknown, start, 1);
    }

    private static bool IsIdentifierStartChar(char c) { return char.IsLetter(c) || c == '_'; }
    private static bool IsIdentifierChar(char c) { return char.IsLetterOrDigit(c) || c == '_' || c == '.'; }
} 