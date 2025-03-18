using System.Collections.Immutable;
using System.Globalization;

namespace MageeSoft.Paradox.Clausewitz.Save.Parser;

public class Parser
{
    private readonly Lexer _lexer;

    private Token _currentToken;

    public Parser(string input)
    {        
        ArgumentException.ThrowIfNullOrWhiteSpace(input, nameof(input));

        _lexer = new Lexer(input);
        _currentToken = _lexer.NextToken();
    }
    private void Eat(TokenType type)
    {
        if (_currentToken.Type == type)
            _currentToken = _lexer.NextToken();
        else
            throw new Exception($"Unexpected token: expected {type}, got {_currentToken.Type} ('{_currentToken.Text}')");
    }
    // Top-level parse: a series of key=value pairs.
    public SaveObject Parse()
    {
        ImmutableArray<KeyValuePair<string, SaveElement>>.Builder builder = ImmutableArray.CreateBuilder<KeyValuePair<string, SaveElement>>();

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            // Skip whitespace and newlines
            while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
            {
                _currentToken = _lexer.NextToken();
            }

            if (_currentToken.Type == TokenType.EndOfFile)
                break;

            if (_currentToken.Type == TokenType.Identifier)
            {
                string key = _currentToken.Text;
                Eat(TokenType.Identifier);

                // Skip whitespace and newlines before equals
                while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
                {
                    _currentToken = _lexer.NextToken();
                }

                if (_currentToken.Type == TokenType.Equals)
                {
                    // Don't append equals to the key, just consume it
                    Eat(TokenType.Equals);
                }

                SaveElement value = ParseValue();
                builder.Add(new KeyValuePair<string, SaveElement>(key, value));
            }
            else
            {
                _currentToken = _lexer.NextToken();
            }
        }

        return new SaveObject(builder.ToImmutable());
    }
    // Parses a value (block or primitive) and infers its type.
    private SaveElement ParseValue()
    {
        // Skip whitespace and newlines before value
        while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
        {
            _currentToken = _lexer.NextToken();
        }

        if (_currentToken.Type == TokenType.CurlyOpen)
            return ParseBlock();

        if (_currentToken.Type == TokenType.StringLiteral)
        {
            string text = _currentToken.Text;

            Eat(TokenType.StringLiteral);

            // Skip whitespace and newlines after value
            while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
            {
                _currentToken = _lexer.NextToken();
            }

            if (Guid.TryParse(text, out Guid guid))
                return new Scalar<Guid>(text, guid);

            if (DateOnly.TryParseExact(text, "yyyy.M.d", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly yyyymd))
                return new Scalar<DateOnly>(text, yyyymd);
            
            if (DateOnly.TryParseExact(text, "yyyy.MM.d", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly yyyymmd))
                return new Scalar<DateOnly>(text, yyyymmd);

            return new Scalar<string>(text, text);
        }
        else if (_currentToken.Type == TokenType.NumberLiteral || _currentToken.Type == TokenType.Identifier)
        {
            string text = _currentToken.Text;
            TokenType origType = _currentToken.Type;

            Eat(origType);

            // Skip whitespace and newlines after value
            while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
            {
                _currentToken = _lexer.NextToken();
            }

            if (origType == TokenType.Identifier)
            {
                if (string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase))
                    return new Scalar<bool>(text, true);

                if (string.Equals(text, "no", StringComparison.OrdinalIgnoreCase))
                    return new Scalar<bool>(text, false);

                return new Scalar<string>(text, text);
            }
            else // NumberLiteral
            {
                if (text.Contains('.'))
                {
                    if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                        return new Scalar<float>(text, f);
                    
                    return new Scalar<string>(text, text);
                }
                else
                {
                    if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out int i32))
                        return new Scalar<int>(text, i32);

                    if (long.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out long i64))
                        return new Scalar<long>(text, i64);

                    return new Scalar<string>(text, text);
                }
            }
        }
        else if (_currentToken.Type == TokenType.CurlyClose)
        {
            // Handle empty blocks by returning an empty object
            return new SaveObject([]);
        }
        else if (_currentToken.Type == TokenType.Equals)
        {
            // Handle equals token in value by treating it as part of the value
            string text = _currentToken.Text;
            Eat(TokenType.Equals);
            return new Scalar<string>(text, text);
        }
        else if (_currentToken.Type == TokenType.EndOfFile)
        {
            // Handle end of file token by returning an empty string
            return new Scalar<string>("", "");
        }
        else
        {
            throw new Exception($"Unexpected token in value: {_currentToken.Type} ('{_currentToken.Text}')");
        }
    }
    
    // Parses a block delimited by '{' and '}'. Supports mixed content.
    private SaveElement ParseBlock()
    {
        Eat(TokenType.CurlyOpen);
        
        // Skip whitespace and newlines after opening brace
        while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
        {
            _currentToken = _lexer.NextToken();
        }
        
        // Handle empty block
        if (_currentToken.Type == TokenType.CurlyClose)
        {
            Eat(TokenType.CurlyClose);
            return new SaveObject([]);
        }
        
        var properties = new List<KeyValuePair<string, SaveElement>>();
        var values = new List<SaveElement>();
        
        while (_currentToken.Type != TokenType.CurlyClose && _currentToken.Type != TokenType.EndOfFile)
        {
            // Skip whitespace and newlines
            while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
            {
                _currentToken = _lexer.NextToken();
            }

            if (_currentToken.Type == TokenType.CurlyClose || _currentToken.Type == TokenType.EndOfFile)
                break;

            // Check if this is a property (key=value) or just a value
            bool isProperty = false;
            if (_currentToken.Type == TokenType.Identifier ||
                _currentToken.Type == TokenType.NumberLiteral ||
                _currentToken.Type == TokenType.StringLiteral)
            {
                // Look ahead to see if there's an equals sign
                Token next = _lexer.PeekToken();
                while (next.Type == TokenType.Whitespace || next.Type == TokenType.NewLine)
                {
                    _lexer.NextToken(); // Consume the whitespace/newline
                    next = _lexer.PeekToken();
                }
                if (next.Type == TokenType.Equals)
                    isProperty = true;
            }
            
            if (isProperty)
            {
                // Handle property (key=value)
                string key = _currentToken.Text;
                Eat(_currentToken.Type);
                
                // Skip whitespace and newlines before equals
                while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
                {
                    _currentToken = _lexer.NextToken();
                }
                
                // Don't append equals to the key, just consume it
                Eat(TokenType.Equals);
                
                // Skip whitespace and newlines after equals
                while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
                {
                    _currentToken = _lexer.NextToken();
                }
                
                SaveElement val = ParseValue();
                properties.Add(new KeyValuePair<string, SaveElement>(key, val));
            }
            else
            {
                // Handle value (no key)
                SaveElement val = ParseValue();
                values.Add(val);
            }
        }
        
        if (_currentToken.Type == TokenType.CurlyClose)
            Eat(TokenType.CurlyClose);

        // Determine what type of element to return based on content
        if (properties.Count > 0 && values.Count > 0)
        {
            // Mixed content - convert values to properties with numeric keys
            for (int i = 0; i < values.Count; i++)
            {
                string autoKey = i.ToString();
                properties.Add(new KeyValuePair<string, SaveElement>(autoKey, values[i]));
            }
            
            return new SaveObject(properties.ToImmutableArray());
        }
        else if (properties.Count > 0)
        {
            // Only properties - return as object
            return new SaveObject(properties.ToImmutableArray());
        }
        else
        {
            // Only values - return as array
            return new SaveArray(values.ToImmutableArray());
        }
    }
}