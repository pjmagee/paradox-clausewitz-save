using System.Runtime.CompilerServices;

namespace MageeSoft.PDX.CE2;

/// <summary>
/// High-performance lexer for Paradox save files that operates on ReadOnlySpan.
/// </summary>
public ref struct PdxLexer
{
    private readonly ReadOnlySpan<char> _text;
    private int _position;
    
    /// <summary>
    /// Creates a new lexer for the given text.
    /// </summary>
    public PdxLexer(ReadOnlySpan<char> text)
    {
        _text = text;
        _position = 0;
    }
    
    /// <summary>
    /// Gets the current position in the text.
    /// </summary>
    public int Position => _position;
    
    /// <summary>
    /// Sets the position to a new location.
    /// </summary>
    public void SetPosition(int position)
    {
        _position = Math.Clamp(position, 0, _text.Length);
    }
    
    /// <summary>
    /// Gets the current character without advancing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char GetCurrent() => _position < _text.Length ? _text[_position] : '\0';
    
    /// <summary>
    /// Looks ahead by the specified offset without advancing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char PeekAhead(int offset = 1) => (_position + offset) < _text.Length ? _text[_position + offset] : '\0';
    
    /// <summary>
    /// Advances the position by the specified count.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance(int count = 1)
    {
        _position = Math.Min(_position + count, _text.Length);
    }
    
    /// <summary>
    /// Gets the next token from the input.
    /// </summary>
    public PdxToken NextToken()
    {
        if (_position >= _text.Length)
            return new PdxToken(PdxTokenType.EndOfFile, _position, 0);
            
        char c = GetCurrent();
        int start = _position;
        
        // Handle whitespace
        if (char.IsWhiteSpace(c) && c != '\r' && c != '\n')
        {
            while (_position < _text.Length && char.IsWhiteSpace(GetCurrent()) && GetCurrent() != '\r' && GetCurrent() != '\n')
                Advance();
            return new PdxToken(PdxTokenType.Whitespace, start, _position - start);
        }
        
        // Handle newlines
        if (c == '\r' || c == '\n')
        {
            if (c == '\r' && PeekAhead() == '\n')
                Advance(2); // Skip CRLF
            else
                Advance();  // Skip CR or LF
            return new PdxToken(PdxTokenType.NewLine, start, _position - start);
        }
        
        // Handle quoted strings
        if (c == '"')
        {
            Advance(); // Skip opening quote
            
            // Find the closing quote, handling escaped quotes
            while (_position < _text.Length && GetCurrent() != '"')
            {
                if (GetCurrent() == '\\' && PeekAhead() == '"')
                    Advance(2); // Skip escape and quote
                else
                    Advance();
            }
            
            // Skip closing quote if present
            if (_position < _text.Length && GetCurrent() == '"')
                Advance();
                
            return new PdxToken(PdxTokenType.StringLiteral, start, _position - start);
        }
        
        // Handle special characters
        if (c == '{') { Advance(); return new PdxToken(PdxTokenType.CurlyOpen, start, 1); }
        if (c == '}') { Advance(); return new PdxToken(PdxTokenType.CurlyClose, start, 1); }
        if (c == '=') { Advance(); return new PdxToken(PdxTokenType.Equals, start, 1); }
        
        // Handle numbers
        if (char.IsDigit(c) || ((c == '-' || c == '+') && char.IsDigit(PeekAhead())) || (c == '.' && char.IsDigit(PeekAhead())))
        {
            // Handle sign
            if (c == '-' || c == '+')
                Advance();
                
            // Handle digits before decimal
            while (_position < _text.Length && char.IsDigit(GetCurrent()))
                Advance();
                
            // Handle decimal point and digits after
            if (_position < _text.Length && GetCurrent() == '.')
            {
                Advance();
                while (_position < _text.Length && char.IsDigit(GetCurrent()))
                    Advance();
            }
            
            // Handle exponent
            if (_position < _text.Length && (GetCurrent() == 'e' || GetCurrent() == 'E'))
            {
                Advance();
                if (_position < _text.Length && (GetCurrent() == '+' || GetCurrent() == '-'))
                    Advance();
                while (_position < _text.Length && char.IsDigit(GetCurrent()))
                    Advance();
            }
            
            return new PdxToken(PdxTokenType.NumberLiteral, start, _position - start);
        }
        
        // Handle identifiers
        if (IsIdentifierStart(c))
        {
            Advance();
            while (_position < _text.Length && IsIdentifierPart(GetCurrent()))
                Advance();
            return new PdxToken(PdxTokenType.Identifier, start, _position - start);
        }
        
        // Handle unknown characters
        Advance();
        return new PdxToken(PdxTokenType.Unknown, start, 1);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIdentifierStart(char c) => char.IsLetter(c) || c == '_';
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIdentifierPart(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == ':';
} 