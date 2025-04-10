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
        int start = 0;
        if (span.Length >= 3 && span[0] == 0xEF && span[1] == 0xBB && span[2] == 0xBF)
        {
            start = 3;
        }
        
        // Skip whitespace at the beginning
        while (start < span.Length && char.IsWhiteSpace(span[start]))
        {
            start++;
        }
        
        // If we reached the end, the entire content is whitespace
        if (start >= span.Length)
        {
            return ReadOnlyMemory<char>.Empty;
        }
        
        // Skip whitespace at the end
        int end = span.Length - 1;
        while (end > start && char.IsWhiteSpace(span[end]))
        {
            end--;
        }
        
        // Return the trimmed slice
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
                // Get the identifier using ReadOnlySpan instead of creating a string immediately
                ReadOnlySpan<char> keySpan = GetCurrentTokenSpan();
                ConsumeToken(); // Eat identifier

                SkipWhitespaceAndNewlines();

                if (_currentToken.Type == PdxTokenType.Equals)
                {
                    ConsumeToken(); // Eat equals
                }

                PdxElement value = ParseValue();
                
                // Convert span to string only when adding to the final collection
                items.Add(new KeyValuePair<string, PdxElement>(keySpan.ToString(), value));
            }
            else if (_currentToken.Type == PdxTokenType.StringLiteral)
            {
                // Handle string literal keys (with quotes)
                string key;
                if (_currentToken.ValueMemory.HasValue)
                {
                    // Use the value memory without allocating a new string if possible
                    key = _currentToken.ValueMemory.Value.Span.ToString();
                }
                else
                {
                    // Fallback to using the raw span if needed
                    key = GetCurrentTokenSpan().ToString();
                }
                
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
            // Use ValueMemory to avoid creating a string when possible
            ReadOnlySpan<char> stringSpan;
            if (_currentToken.ValueMemory.HasValue)
            {
                stringSpan = _currentToken.ValueMemory.Value.Span;
            }
            else
            {
                stringSpan = GetCurrentTokenSpan();
            }
            
            // Store original token to work with after consuming
            var originalToken = _currentToken;
            ConsumeToken(); // Consume the token
            
            // Try to parse special types using spans to avoid string allocations
            if (TryParseGuid(stringSpan, out Guid guid))
                return new PdxScalar<Guid>(guid);

            if (TryParseDate(stringSpan, out DateTime date))
                return new PdxScalar<DateTime>(date);

            // Finally convert to string for scalar value
            return new PdxScalar<string>(stringSpan.ToString());
        }

        if (_currentToken.Type == PdxTokenType.NumberLiteral || _currentToken.Type == PdxTokenType.Identifier)
        {
            ReadOnlySpan<char> tokenSpan = GetCurrentTokenSpan();
            PdxTokenType origType = _currentToken.Type;
            ConsumeToken(); // Consume the token

            if (origType == PdxTokenType.Identifier)
            {
                // Check for yes/no boolean values
                if (tokenSpan.Equals("yes".AsSpan(), StringComparison.OrdinalIgnoreCase))
                    return new PdxScalar<bool>(true);

                if (tokenSpan.Equals("no".AsSpan(), StringComparison.OrdinalIgnoreCase))
                    return new PdxScalar<bool>(false);

                return new PdxScalar<string>(tokenSpan.ToString());
            }
            else // NumberLiteral
            {
                if (tokenSpan.Contains('.'))
                {
                    if (float.TryParse(tokenSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                        return new PdxScalar<float>(f);
                    
                    return new PdxScalar<string>(tokenSpan.ToString());
                }
                else
                {
                    if (int.TryParse(tokenSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out int i32))
                        return new PdxScalar<int>(i32);

                    if (long.TryParse(tokenSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out long i64))
                        return new PdxScalar<long>(i64);

                    return new PdxScalar<string>(tokenSpan.ToString());
                }
            }
        }
        else if (_currentToken.Type == PdxTokenType.Equals)
        {
            // Handle equals as a value (just like Parser.cs does)
            ReadOnlySpan<char> tokenSpan = GetCurrentTokenSpan();
            ConsumeToken();
            return new PdxScalar<string>(tokenSpan.ToString());
        }

        // Default fallback for unrecognized token types - skip and return empty string
        ReadOnlySpan<char> defaultSpan = GetCurrentTokenSpan();
        ConsumeToken();
        return new PdxScalar<string>(defaultSpan.ToString());
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
                ReadOnlySpan<char> keySpan;
                
                if (_currentToken.Type == PdxTokenType.StringLiteral && _currentToken.ValueMemory.HasValue)
                {
                    // Use the processed memory for quoted keys
                    keySpan = _currentToken.ValueMemory.Value.Span;
                }
                else
                {
                    // Use the span directly for regular keys
                    keySpan = GetCurrentTokenSpan();
                }
                
                // Store token information to create the string key later
                var tokenType = _currentToken.Type;
                
                ConsumeToken(); // Eat key
                
                SkipWhitespaceAndNewlines();
                
                if (_currentToken.Type == PdxTokenType.Equals)
                    ConsumeToken(); // Eat equals
                
                SkipWhitespaceAndNewlines();
                
                PdxElement val = ParseValue();
                
                // Convert span to string only when adding to the collection
                string key = keySpan.ToString();
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

    // Helper for parsing GUID using span to avoid string allocation
    private static bool TryParseGuid(ReadOnlySpan<char> span, out Guid result)
    {
        return Guid.TryParse(span, out result);
    }

    // Helper for parsing multiple date formats efficiently using spans
    private static bool TryParseDate(ReadOnlySpan<char> span, out DateTime date)
    {
        // Try common formats used in Paradox saves
        if (DateTime.TryParseExact(span, "yyyy.M.d", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
            DateTime.TryParseExact(span, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
            DateTime.TryParseExact(span, "yyyy.MM.d", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
            DateTime.TryParseExact(span, "yyyy.M.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }
        
        date = default;
        return false;
    }
} 