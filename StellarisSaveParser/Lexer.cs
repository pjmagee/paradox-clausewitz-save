using System.Text;

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
        while (char.IsWhiteSpace(Current))
            Next();

        if (_position >= _input.Length)
            return new Token(TokenType.EndOfFile, "");

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
                sb.Append(Current);
                Next();
            }

            if (Current == '"')
                Next(); // skip closing quote

            return new Token(TokenType.StringLiteral, sb.ToString());
        }

        if (char.IsLetter(Current) || char.IsDigit(Current) || Current == '-' || Current == '%')
        {
            StringBuilder sb = new StringBuilder();

            while (char.IsLetterOrDigit(Current) || Current == '_' || Current == '-' || Current == '%' || Current == '.')
            {
                sb.Append(Current);
                Next();
            }

            string tokenText = sb.ToString();

            bool isNumber = true;

            foreach (char c in tokenText)
            {
                if (!(char.IsDigit(c) || c == '-' || c == '.'))
                {
                    isNumber = false;
                    break;
                }
            }

            return isNumber ? new Token(TokenType.NumberLiteral, tokenText) : new Token(TokenType.Identifier, tokenText);
        }

        Next();

        return NextToken();
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
