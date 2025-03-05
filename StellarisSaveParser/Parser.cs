using System;
using System.Collections.Generic;
using System.Globalization;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;
    public Parser(string input)
    {
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
    public Element Parse()
    {
        var root = new Object();

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            if (_currentToken.Type == TokenType.Identifier)
            {
                string key = _currentToken.Text;
                Eat(TokenType.Identifier);

                if (_currentToken.Type == TokenType.Equals)
                    Eat(TokenType.Equals);

                Element value = ParseValue();
                root.Properties.Add(new KeyValuePair<string, Element>(key, value));
            }
            else
            {
                _currentToken = _lexer.NextToken();
            }
        }

        return root;
    }
    // Parses a value (block or primitive) and infers its type.
    private Element ParseValue()
    {
        if (_currentToken.Type == TokenType.CurlyOpen)
            return ParseBlock();

        if (_currentToken.Type == TokenType.StringLiteral)
        {
            string text = _currentToken.Text;

            Eat(TokenType.StringLiteral);

            if (Guid.TryParse(text, out Guid guid))
                return new Scalar(text, ValueType.Guid, guid);

            if (DateOnly.TryParseExact(text, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dt))
                return new Scalar(text, ValueType.Date, dt);

            return new Scalar(text, ValueType.String, text);
        }
        else if (_currentToken.Type == TokenType.NumberLiteral || _currentToken.Type == TokenType.Identifier)
        {
            string text = _currentToken.Text;
            TokenType origType = _currentToken.Type;

            Eat(origType);

            if (origType == TokenType.Identifier)
            {
                if (string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase))
                    return new Scalar(text, ValueType.Boolean, true);

                if (string.Equals(text, "no", StringComparison.OrdinalIgnoreCase))
                    return new Scalar(text, ValueType.Boolean, false);

                return new Scalar(text, ValueType.Identifier, text);
            }
            else // NumberLiteral
            {
                if (text.Contains("."))
                {
                    if (Single.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out Single f))
                        return new Scalar(text, ValueType.Float, f);

                    if (Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out Double d))
                        return new Scalar(text, ValueType.Double, d);

                    return new Scalar(text, ValueType.Identifier, text);
                }
                else
                {
                    if (Int16.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out Int16 i16))
                        return new Scalar(text, ValueType.Int16, i16);

                    if (Int32.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out Int32 i32))
                        return new Scalar(text, ValueType.Int32, i32);

                    if (Int64.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out Int64 i64))
                        return new Scalar(text, ValueType.Int64, i64);

                    return new Scalar(text, ValueType.Identifier, text);
                }
            }
        }
        else
        {
            throw new Exception($"Unexpected token in value: {_currentToken.Type} ('{_currentToken.Text}')");
        }
    }
    
    // Parses a block delimited by '{' and '}'. Supports mixed content.
    private Element ParseBlock()
    {
        Eat(TokenType.CurlyOpen);
        if (_currentToken.Type == TokenType.CurlyClose)
        {
            Eat(TokenType.CurlyClose);
            return new Object();
        }
        var properties = new List<KeyValuePair<string, Element>>();
        var values = new List<Element>();
        while (_currentToken.Type != TokenType.CurlyClose)
        {
            bool isProperty = false;
            if (_currentToken.Type == TokenType.Identifier ||
                _currentToken.Type == TokenType.NumberLiteral ||
                _currentToken.Type == TokenType.StringLiteral)
            {
                Token next = _lexer.PeekToken();
                if (next.Type == TokenType.Equals)
                    isProperty = true;
            }
            if (isProperty)
            {
                string key = _currentToken.Text;
                Eat(_currentToken.Type);
                Eat(TokenType.Equals);
                Element val = ParseValue();
                properties.Add(new KeyValuePair<string, Element>(key, val));
            }
            else
            {
                Element val = ParseValue();
                values.Add(val);
            }
        }
        Eat(TokenType.CurlyClose);
        if (properties.Count > 0 && values.Count > 0)
        {
            for (int i = 0; i < values.Count; i++)
            {
                string autoKey = i.ToString();
                properties.Add(new KeyValuePair<string, Element>(autoKey, values[i]));
            }
            var merged = new Object();
            merged.Properties.AddRange(properties);
            return merged;
        }
        else if (properties.Count > 0)
        {
            var obj = new Object();
            obj.Properties.AddRange(properties);
            return obj;
        }
        else
        {
            var arr = new Array();
            arr.Items.AddRange(values);
            return arr;
        }
    }
}
