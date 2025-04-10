namespace MageeSoft.PDX.CE2;

/// <summary>
/// Lexer for the PdxSaveReader.
/// </summary>
internal struct PdxLexer
{
    private readonly ReadOnlyMemory<char> _inputMemory;
    private int _position;

    public int CurrentPosition => _position;

    public PdxLexer(ReadOnlyMemory<char> input)
    {
        _inputMemory = input;
        _position = 0;
    }

    public void SetPosition(int position)
    {
        _position = Math.Max(0, Math.Min(position, _inputMemory.Length));
    }

    private char Current => _position < _inputMemory.Length ? _inputMemory.Span[_position] : '\0';
    private char Peek(int offset = 1) => _position + offset < _inputMemory.Length ? _inputMemory.Span[_position + offset] : '\0';

    private void Advance(int count = 1)
    {
        _position = Math.Min(_position + count, _inputMemory.Length);
    }

    public PdxToken NextToken()
    {
        // Skip any BOM or other zero-width characters at the start
        if (_position == 0 && _inputMemory.Length >= 3 && 
            _inputMemory.Span[0] == 0xEF && _inputMemory.Span[1] == 0xBB && _inputMemory.Span[2] == 0xBF)
        {
            _position = 3;
        }
        
        // Return EOF at end of input
        if (_position >= _inputMemory.Length)
        {
            return new PdxToken(PdxTokenType.EndOfFile, _position, 0);
        }

        char c = Current;
        int textOffset = _position; // Start of token

        // Handle whitespace
        if (char.IsWhiteSpace(c) && c != '\n' && c != '\r')
        {
            while (_position < _inputMemory.Length && 
                   char.IsWhiteSpace(Current) && Current != '\n' && Current != '\r')
            {
                Advance();
            }
            return new PdxToken(PdxTokenType.Whitespace, textOffset, _position - textOffset);
        }

        // Handle newlines
        if (c == '\n' || c == '\r')
        {
            // Handle \r\n sequence as a single token
            if (c == '\r' && Peek() == '\n')
            {
                Advance(2);
            }
            else
            {
                Advance();
            }
            return new PdxToken(PdxTokenType.NewLine, textOffset, _position - textOffset);
        }

        // Handle string literals
        if (c == '"')
        {
            Advance(); // Skip opening quote
            int start = _position;
            
            // Find the closing quote or end of input
            while (_position < _inputMemory.Length && Current != '"')
            {
                // Handle escape sequences
                if (Current == '\\' && Peek() == '"')
                {
                    Advance(); // Skip the backslash
                }
                Advance();
            }
            
            // Extract string content (without quotes)
            string processedString = _inputMemory.Span.Slice(start, _position - start).ToString();
            
            // Skip closing quote if present
            if (_position < _inputMemory.Length && Current == '"')
            {
                Advance();
            }
            
            return new PdxToken(PdxTokenType.StringLiteral, textOffset, _position - textOffset, processedString);
        }

        // Handle operators
        if (c == '{')
        {
            Advance();
            return new PdxToken(PdxTokenType.CurlyOpen, textOffset, 1);
        }

        if (c == '}')
        {
            Advance();
            return new PdxToken(PdxTokenType.CurlyClose, textOffset, 1);
        }

        if (c == '=')
        {
            Advance();
            return new PdxToken(PdxTokenType.Equals, textOffset, 1);
        }

        // Handle numbers
        if (char.IsDigit(c) || c == '-' || c == '+' || c == '.')
        {
            // Special case for a lone decimal point
            if (c == '.' && !char.IsDigit(Peek()))
            {
                Advance();
                return new PdxToken(PdxTokenType.Unknown, textOffset, 1);
            }

            // Handle sign
            if (c == '-' || c == '+')
            {
                Advance();
            }

            // Scan digits before decimal point
            while (_position < _inputMemory.Length && char.IsDigit(Current))
            {
                Advance();
            }

            // Handle decimal point and following digits
            if (_position < _inputMemory.Length && Current == '.')
            {
                Advance();
                // Scan digits after decimal point
                while (_position < _inputMemory.Length && char.IsDigit(Current))
                {
                    Advance();
                }
            }

            return new PdxToken(PdxTokenType.NumberLiteral, textOffset, _position - textOffset);
        }

        // Handle identifiers (including yes/no keywords)
        if (IsIdentifierStart(c))
        {
            // First character already checked with IsIdentifierStart
            Advance();
            
            // Scan the rest of the identifier
            while (_position < _inputMemory.Length && IsIdentifierPart(Current))
            {
                Advance();
            }
            
            return new PdxToken(PdxTokenType.Identifier, textOffset, _position - textOffset);
        }

        // Unrecognized character, return as unknown token
        Advance();
        return new PdxToken(PdxTokenType.Unknown, textOffset, 1);
    }

    // Helper function to determine if a character can start an identifier
    private static bool IsIdentifierStart(char c)
    {
        return char.IsLetter(c) || c == '_';
    }

    // Helper function to determine if a character can be part of an identifier
    private static bool IsIdentifierPart(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.';
    }
}