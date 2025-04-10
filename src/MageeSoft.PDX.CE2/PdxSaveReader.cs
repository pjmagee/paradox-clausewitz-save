using System.Globalization;
using System.Text; // Potentially needed if creating strings from spans

namespace MageeSoft.PDX.CE2;

/// <summary>
/// Reads Paradox Clausewitz Engine save file data using high-performance zero-allocation techniques.
/// </summary>
public class PdxSaveReader
{
    private readonly ReadOnlyMemory<char> _inputMemory;
    private PdxLexer _lexer; 
    private PdxToken _currentToken;

    private PdxSaveReader(ReadOnlyMemory<char> inputMemory)
    {
        _inputMemory = inputMemory;
        _lexer = new PdxLexer(inputMemory);
        _currentToken = _lexer.NextToken(); // Initialize _currentToken
    }

    /// <summary>
    /// Reads the input memory representing Paradox save data and converts it to a SaveObject.
    /// </summary>
    /// <param name="inputMemory">The save file content as ReadOnlyMemory</param>
    /// <returns>A SaveObject representing the parsed save data</returns>
    /// <exception cref="FormatException">Thrown if parsing fails due to invalid format.</exception>
    public static PdxObject Read(ReadOnlyMemory<char> inputMemory)
    {
        if (inputMemory.IsEmpty) return new PdxObject(new List<KeyValuePair<string, PdxElement>>());
        var trimmedMemory = TrimBomAndWhitespace(inputMemory);
        if (trimmedMemory.IsEmpty) return new PdxObject(new List<KeyValuePair<string, PdxElement>>());
        return new PdxSaveReader(trimmedMemory).ParseInternal();
    }

    // Handles UTF-8 BOM and trims whitespace
    private static ReadOnlyMemory<char> TrimBomAndWhitespace(ReadOnlyMemory<char> memory)
    {
        var span = memory.Span;
        // Check for UTF-8 BOM (EF BB BF)
        if (span.Length >= 3 && span[0] == 0xEF && span[1] == 0xBB && span[2] == 0xBF)
        {
            span = span.Slice(3);
        }
        // Trim whitespace (equivalent to string.Trim())
        int start = 0;
        while (start < span.Length && char.IsWhiteSpace(span[start]))
        {
            start++;
        }
        int end = span.Length - 1;
        while (end >= start && char.IsWhiteSpace(span[end]))
        {
            end--;
        }
        // Slice the original memory to avoid allocating a new string/array
        if (start > end) return ReadOnlyMemory<char>.Empty; // All whitespace
        return memory.Slice(start, end - start + 1);
    }

    private void ConsumeToken()
    {
        _currentToken = _lexer.NextToken();
    }

    private void SkipWhitespaceAndNewlines()
    {
        while (_currentToken.Type == PdxTokenType.Whitespace || _currentToken.Type == PdxTokenType.NewLine)
        {
            ConsumeToken();
        }
    }

    private ReadOnlySpan<char> GetCurrentTokenSpan() => _inputMemory.Span.Slice(_currentToken.Start, _currentToken.Length);

    // Top-level parse: a series of key=value pairs.
    private PdxObject ParseInternal()
    {
        List<KeyValuePair<string, PdxElement>> items = new();

        while (_currentToken.Type != PdxTokenType.EndOfFile)
        {
            SkipWhitespaceAndNewlines();

            if (_currentToken.Type == PdxTokenType.EndOfFile)
                break;

            if (_currentToken.Type == PdxTokenType.Identifier)
            {
                string key = GetCurrentTokenSpan().ToString();
                ConsumeToken(); // Eat identifier

                SkipWhitespaceAndNewlines();

                if (_currentToken.Type == PdxTokenType.Equals)
                {
                    ConsumeToken(); // Eat equals
                }

                PdxElement value = ParseValue();
                items.Add(new KeyValuePair<string, PdxElement>(key, value));
            }
            else if (_currentToken.Type == PdxTokenType.StringLiteral)
            {
                // Handle string literal keys (with quotes)
                string key = _currentToken.ProcessedString ?? "";
                ConsumeToken(); // Eat quoted key

                SkipWhitespaceAndNewlines();

                if (_currentToken.Type == PdxTokenType.Equals)
                {
                    ConsumeToken(); // Eat equals
                }

                PdxElement value = ParseValue();
                items.Add(new KeyValuePair<string, PdxElement>(key, value));
            }
            else
            {
                // Skip any unrecognized tokens and continue parsing
                // This matches Parser.cs behavior of silently skipping unexpected tokens
                ConsumeToken();
            }
        }

        return new PdxObject(items);
    }

    // Parses a value (block or primitive) and infers its type.
    private PdxElement ParseValue()
    {
        SkipWhitespaceAndNewlines();

        if (_currentToken.Type == PdxTokenType.EndOfFile)
            return new PdxScalar<string>(""); // Handle EOF as empty string, matching Parser.cs

        if (_currentToken.Type == PdxTokenType.CurlyOpen)
            return ParseBlock();

        if (_currentToken.Type == PdxTokenType.CurlyClose)
        {
            // Handle closing brace without opening brace as empty object
            return new PdxObject(new List<KeyValuePair<string, PdxElement>>());
        }

        if (_currentToken.Type == PdxTokenType.StringLiteral)
        {
            // Get the raw string value without processing
            string stringValue = _currentToken.ProcessedString ?? GetCurrentTokenSpan().ToString();
            ConsumeToken(); // Consume the token

            // Handle special types
            if (Guid.TryParse(stringValue, out Guid guid))
                return new PdxScalar<Guid>(guid);

            if (TryParseDate(stringValue, out DateTime date))
                return new PdxScalar<DateTime>(date);

            // Default to string
            return new PdxScalar<string>(stringValue);
        }

        if (_currentToken.Type == PdxTokenType.NumberLiteral || _currentToken.Type == PdxTokenType.Identifier)
        {
            string text = GetCurrentTokenSpan().ToString();
            PdxTokenType origType = _currentToken.Type;
            ConsumeToken(); // Consume the token

            if (origType == PdxTokenType.Identifier)
            {
                if (string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase))
                    return new PdxScalar<bool>(true);

                if (string.Equals(text, "no", StringComparison.OrdinalIgnoreCase))
                    return new PdxScalar<bool>(false);

                return new PdxScalar<string>(text);
            }
            else // NumberLiteral
            {
                if (text.Contains('.'))
                {
                    if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                        return new PdxScalar<float>(f);
                    
                    return new PdxScalar<string>(text);
                }
                else
                {
                    if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out int i32))
                        return new PdxScalar<int>(i32);

                    if (long.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out long i64))
                        return new PdxScalar<long>(i64);

                    return new PdxScalar<string>(text);
                }
            }
        }
        else if (_currentToken.Type == PdxTokenType.Equals)
        {
            // Handle equals as a value (just like Parser.cs does)
            string text = GetCurrentTokenSpan().ToString();
            ConsumeToken();
            return new PdxScalar<string>(text);
        }

        // Default fallback for unrecognized token types - skip and return empty string
        string defaultText = GetCurrentTokenSpan().ToString();
        ConsumeToken();
        return new PdxScalar<string>(defaultText);
    }

    // Parses a block delimited by '{' and '}'. Determines if it's an Object or Array.
    private PdxElement ParseBlock()
    {
        // Eat the opening brace
        ConsumeToken();
        SkipWhitespaceAndNewlines();

        if (_currentToken.Type == PdxTokenType.CurlyClose) // Handle empty block {}
        {
            ConsumeToken(); // Eat '}'
            return new PdxObject(new List<KeyValuePair<string, PdxElement>>());
        }

        var properties = new List<KeyValuePair<string, PdxElement>>();
        var values = new List<PdxElement>();
        
        while (_currentToken.Type != PdxTokenType.CurlyClose && _currentToken.Type != PdxTokenType.EndOfFile)
        {
            SkipWhitespaceAndNewlines();

            if (_currentToken.Type == PdxTokenType.CurlyClose || _currentToken.Type == PdxTokenType.EndOfFile)
                break;

            // Check if this is a property (key=value) or just a value
            bool isProperty = false;
            if (_currentToken.Type == PdxTokenType.Identifier ||
                _currentToken.Type == PdxTokenType.NumberLiteral ||
                _currentToken.Type == PdxTokenType.StringLiteral)
            {
                // Look ahead to see if there's an equals sign
                int savedPosition = _lexer.CurrentPosition;
                PdxToken currentToken = _currentToken;
                
                // Consume the current token
                ConsumeToken();
                
                // Skip any whitespace/newlines
                SkipWhitespaceAndNewlines();
                
                // Check if the next token is '='
                if (_currentToken.Type == PdxTokenType.Equals)
                {
                    isProperty = true;
                    // Restore position
                    _lexer.SetPosition(savedPosition);
                    _currentToken = currentToken;
                }
                else
                {
                    // Restore position
                    _lexer.SetPosition(savedPosition);
                    _currentToken = currentToken;
                }
            }
            
            if (isProperty)
            {
                // Handle property (key=value)
                string key;
                if (_currentToken.Type == PdxTokenType.StringLiteral)
                {
                    // Use the processed string for quoted keys
                    key = _currentToken.ProcessedString ?? "";
                }
                else
                {
                    // Use the span directly for regular keys
                    key = GetCurrentTokenSpan().ToString();
                }
                
                ConsumeToken(); // Eat key
                
                SkipWhitespaceAndNewlines();
                
                if (_currentToken.Type == PdxTokenType.Equals)
                    ConsumeToken(); // Eat equals
                
                SkipWhitespaceAndNewlines();
                
                PdxElement val = ParseValue();
                properties.Add(new KeyValuePair<string, PdxElement>(key, val));
            }
            else
            {
                // Handle value (no key)
                PdxElement val = ParseValue();
                values.Add(val);
            }
        }
        
        if (_currentToken.Type == PdxTokenType.CurlyClose)
            ConsumeToken(); // Eat '}'

        // Determine what type of element to return based on content
        if (properties.Count > 0 && values.Count > 0)
        {
            // Mixed content - convert values to properties with numeric keys
            for (int i = 0; i < values.Count; i++)
            {
                string autoKey = i.ToString();
                properties.Add(new KeyValuePair<string, PdxElement>(autoKey, values[i]));
            }
            
            return new PdxObject(properties);
        }
        else if (properties.Count > 0)
        {
            // Only properties - return as object
            return new PdxObject(properties);
        }
        else
        {
            // Only values - return as array
            return new PdxArray(values);
        }
    }

    // Helper for parsing multiple date formats efficiently
    private static bool TryParseDate(string s, out DateTime date)
    {
        // List common formats used in Paradox saves
        string[] formats = { "yyyy.M.d", "yyyy.MM.dd", "yyyy.MM.d", "yyyy.M.dd" };
        // Use InvariantCulture to ensure consistent parsing regardless of system locale
        return DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
} 