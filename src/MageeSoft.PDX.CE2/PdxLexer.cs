using System.Text;

namespace MageeSoft.PDX.CE2;

/// <summary>
/// Lexer for the PdxSaveReader.
/// Reverted to known-good state.
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
        if (_position >= _inputMemory.Length)
        {
            return new PdxToken(PdxTokenType.EndOfFile, _position, 0);
        }

        char c = Current;
        int start = _position;

        if (char.IsWhiteSpace(c) && c != '\n' && c != '\r')
        {
            while (_position < _inputMemory.Length && char.IsWhiteSpace(Current) && Current != '\n' && Current != '\r') Advance();
            return new PdxToken(PdxTokenType.Whitespace, start, _position - start);
        }

        if (c == '\n' || c == '\r')
        {
            if (c == '\r' && Peek() == '\n') Advance(2);
            else Advance();
            return new PdxToken(PdxTokenType.NewLine, start, _position - start);
        }

        if (c == '"')
        {
            Advance(); // Skip opening quote
            int contentStart = _position;
            StringBuilder? sb = null; 

            while (_position < _inputMemory.Length && Current != '"')
            {
                if (Current == '\\')
                {
                    if (sb == null) 
                    {
                        sb = new StringBuilder(_inputMemory.Span.Slice(contentStart, _position - contentStart).Length); 
                        sb.Append(_inputMemory.Span.Slice(contentStart, _position - contentStart));
                    }
                    Advance(); // Consume backslash
                    if (_position < _inputMemory.Length)
                    {
                        if (Current == '"') sb?.Append('"'); 
                        else sb?.Append('\\').Append(Current); 
                        Advance(); 
                    }
                }
                else
                {
                    sb?.Append(Current);
                    Advance();
                }
            }

            string processedString = sb?.ToString() ?? _inputMemory.Span.Slice(contentStart, _position - contentStart).ToString();
            
            if (_position < _inputMemory.Length && Current == '"') Advance(); // Skip closing quote
            
            return new PdxToken(PdxTokenType.StringLiteral, start, _position - start, processedString);
        }

        if (c == '{') { Advance(); return new PdxToken(PdxTokenType.CurlyOpen, start, 1); }
        if (c == '}') { Advance(); return new PdxToken(PdxTokenType.CurlyClose, start, 1); }
        if (c == '=') { Advance(); return new PdxToken(PdxTokenType.Equals, start, 1); }

        if (char.IsDigit(c) || (c == '-' || c == '+') && char.IsDigit(Peek()) || c == '.' && char.IsDigit(Peek()))
        {
            int numStart = _position;
            if (c == '-' || c == '+') Advance();
            while (_position < _inputMemory.Length && char.IsDigit(Current)) Advance();
            if (_position < _inputMemory.Length && Current == '.')
            {
                Advance();
                while (_position < _inputMemory.Length && char.IsDigit(Current)) Advance();
            }
             if (_position < _inputMemory.Length && (Current == 'e' || Current == 'E')) // Handle exponent
            {
                Advance();
                if (_position < _inputMemory.Length && (Current == '-' || Current == '+')) Advance();
                while (_position < _inputMemory.Length && char.IsDigit(Current)) Advance();
            }
            return new PdxToken(PdxTokenType.NumberLiteral, numStart, _position - numStart);
        }

        if (IsIdentifierStart(c))
        {
            int idStart = _position;
            Advance(); 
            while (_position < _inputMemory.Length && IsIdentifierPart(Current)) Advance();
            return new PdxToken(PdxTokenType.Identifier, idStart, _position - idStart);
        }

        Advance();
        return new PdxToken(PdxTokenType.Unknown, start, 1);
    }

    private static bool IsIdentifierStart(char c) => char.IsLetter(c) || c == '_';
    private static bool IsIdentifierPart(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == ':';
}