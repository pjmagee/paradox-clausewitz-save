using System.Text;

namespace MageeSoft.Paradox.Clausewitz.Save.Parser;

public class Lexer
{
    private readonly string _input;
    private int _position;
    
    public Lexer(string input)
    {
        _input = input;
        _position = 0;
    }
    
    private char Current => _position < _input.Length ? _input[_position] : '\0';
    private void Next() => _position++;

    public Token NextToken()
    {
        if (_position >= _input.Length)
            return new Token(TokenType.EndOfFile, "");

        if (Current == '\n' || Current == '\r')
        {
            var start = _position;
            Next();
            if (Current == '\n' && _position > 0 && _input[_position - 1] == '\r')
                Next();
            return new Token(TokenType.NewLine, _input[start].ToString());
        }

        if (char.IsWhiteSpace(Current))
        {
            var start = _position;
            while (char.IsWhiteSpace(Current) && Current != '\n' && Current != '\r')
                Next();
            return new Token(TokenType.Whitespace, _input[start.._position]);
        }

        if (Current == '{')
        {
            Next();
            return new Token(TokenType.CurlyOpen, "{");
        }

        if (Current == '}')
        {
            Next();
            return new Token(TokenType.CurlyClose, "}");
        }

        if (Current == '=')
        {
            Next();
            return new Token(TokenType.Equals, "=");
        }

        if (Current == '"')
        {
            StringBuilder sb = new StringBuilder();

            Next(); // skip opening quote

            while (Current != '"' && Current != '\0')
            {
                if (Current == '\\' && _position + 1 < _input.Length)
                {
                    Next(); // skip backslash
                    if (Current == '"')
                        sb.Append('"');
                    else
                        sb.Append('\\').Append(Current);
                }
                else
                {
                    sb.Append(Current);
                }
                Next();
            }

            if (Current == '"')
                Next(); // skip closing quote

            return new Token(TokenType.StringLiteral, sb.ToString());
        }

        if (char.IsLetter(Current) || Current == '_' || Current == '%' || (char.IsDigit(Current) && !IsNumeric()))
        {
            // This is an identifier
            StringBuilder sb = new StringBuilder();
            
            while (Current != '\0' && !char.IsWhiteSpace(Current) && Current != '{' && Current != '}' && Current != '=')
            {
                sb.Append(Current);
                Next();
            }
            
            return new Token(TokenType.Identifier, sb.ToString());
        }
        
        if (char.IsDigit(Current) || Current == '-' || Current == '.')
        {
            // This is a number
            StringBuilder sb = new StringBuilder();
            
            bool hasDecimal = Current == '.';
            
            while (Current != '\0' && (char.IsDigit(Current) || Current == '-' || Current == '.'))
            {
                if (Current == '.')
                {
                    if (hasDecimal) // Second decimal point is not valid in a number
                        break;
                    hasDecimal = true;
                }
                
                sb.Append(Current);
                Next();
            }
            
            return new Token(TokenType.NumberLiteral, sb.ToString());
        }

        Next();
        return NextToken();
    }
    
    // Helper method to check if the current position starts a numeric value
    private bool IsNumeric()
    {
        if (Current == '-' || Current == '.' || char.IsDigit(Current))
        {
            // Look ahead to see if this is a valid number
            int i = _position;
            bool hasDecimal = Current == '.';
            bool hasDigit = char.IsDigit(Current);
            
            while (i < _input.Length && (char.IsDigit(_input[i]) || _input[i] == '-' || _input[i] == '.'))
            {
                if (_input[i] == '.')
                {
                    if (hasDecimal) // Second decimal point is not valid in a number
                        return false;
                    hasDecimal = true;
                }
                else if (char.IsDigit(_input[i]))
                {
                    hasDigit = true;
                }
                i++;
            }
            
            // A valid number must have at least one digit
            return hasDigit;
        }
        
        return false;
    }

    // Peek without consuming.
    public Token PeekToken()
    {
        int savedPosition = _position;
        Token token = NextToken();
        _position = savedPosition;
        return token;
    }
}