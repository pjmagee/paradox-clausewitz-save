using System.Globalization;
using System.Text; // Potentially needed if creating strings from spans

namespace MageeSoft.PDX.CE2;

/// <summary>
/// Reads Paradox Clausewitz Engine save file data using high-performance zero-allocation techniques.
/// </summary>
public class PdxSaveReader
{
    private readonly ReadOnlyMemory<char> _inputMemory;
    private Lexer _lexer; // Lexer is a struct
    private Token _currentToken;

    private PdxSaveReader(ReadOnlyMemory<char> inputMemory)
    {
        _inputMemory = inputMemory;
        _lexer = new Lexer(inputMemory);
        ConsumeToken(); // Initialize _currentToken
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
        while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.NewLine)
        {
            ConsumeToken();
        }
    }

    private ReadOnlySpan<char> GetCurrentTokenSpan() => _inputMemory.Span.Slice(_currentToken.Start, _currentToken.Length);

    // Top-level parse: a series of key=value pairs.
    private PdxObject ParseInternal()
    {
        List<KeyValuePair<string, PdxElement>> items = new();

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            SkipWhitespaceAndNewlines();
            if (_currentToken.Type == TokenType.EndOfFile) break;

            // Expect Key = Value structure at root
            if (_currentToken.Type == TokenType.Identifier)
            {
                string key = GetCurrentTokenSpan().ToString();
                ConsumeToken(); // Eat identifier

                SkipWhitespaceAndNewlines();

                if (_currentToken.Type == TokenType.Equals)
                {
                    ConsumeToken(); // Eat equals
                }
                else
                {
                    throw new FormatException($"Expected '=' after root key '{key}' but found {_currentToken.Type} at position {_currentToken.Start}");
                }

                PdxElement value = ParseValue();
                items.Add(new KeyValuePair<string, PdxElement>(key, value));
            }
            // Allow root level comments? Need '#' token type in Lexer if so.
            // else if (_currentToken.Type == TokenType.Comment) { ConsumeToken(); continue; }
            else
            {
                 throw new FormatException($"Unexpected token at root level: {_currentToken.Type} at position {_currentToken.Start}");
            }
        }

        return new PdxObject(items);
    }

    // Parses a value (block or primitive) and infers its type.
    private PdxElement ParseValue()
    {
        SkipWhitespaceAndNewlines();

        Token valueToken = _currentToken; // Capture token before consuming
        ReadOnlySpan<char> valueSpan = GetCurrentTokenSpan();

        switch (valueToken.Type)
        {
            case TokenType.CurlyOpen:
                return ParseBlock();

            case TokenType.StringLiteral:
                {
                    // Lexer provides the *unescaped* string value
                    ConsumeToken(); // Consume after getting value
                    string stringValue = valueToken.ProcessedString ?? string.Empty;

                    // Attempt to parse specific formats first
                    if (Guid.TryParse(stringValue, out Guid guid)) return new PdxScalar<Guid>(guid);
                    if (TryParseDate(stringValue, out DateTime date)) return new PdxScalar<DateTime>(date);

                    // Default to string if no specific format matches
                    return new PdxScalar<string>(stringValue);
                }

            case TokenType.NumberLiteral:
                {
                    ConsumeToken(); // Consume after getting span

                    // Try parsing float first if decimal point exists
                    if (valueSpan.IndexOf('.') >= 0)
                    {
                        // Use float.TryParse. CultureInfo.InvariantCulture is crucial.
                        if (float.TryParse(valueSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                            return new PdxScalar<float>(f);
                    }
                    else // No decimal point, try integer types
                    {
                        // Try int first, then long. CultureInfo.InvariantCulture is crucial.
                        if (int.TryParse(valueSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out int i32))
                            return new PdxScalar<int>(i32);
                        if (long.TryParse(valueSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out long i64))
                            return new PdxScalar<long>(i64);
                    }

                    // Fallback: If parsing fails (e.g., invalid format), treat as string.
                    return new PdxScalar<string>(valueSpan.ToString());
                }

            case TokenType.Identifier: // Handles yes, no, none, or other unquoted strings
                {
                    ConsumeToken(); // Consume after getting span

                    if (valueSpan.Equals("yes", StringComparison.OrdinalIgnoreCase)) return new PdxScalar<bool>(true);
                    if (valueSpan.Equals("no", StringComparison.OrdinalIgnoreCase)) return new PdxScalar<bool>(false);

                    // Represent "none" identifier as a specific string scalar.
                    // Consumers (like the source generator) must interpret this specific string value.
                    // This avoids needing a nullable type directly in the base parser model.
                    // It also correctly handles cases where "none" might be a legitimate unquoted string value.
                    return new PdxScalar<string>(valueSpan.ToString());
                }

            case TokenType.CurlyClose: // Handles empty values, e.g., key={} or array={ val1 {} val2 }
                 // This indicates an empty object value in these contexts.
                 // It's consumed by ParseBlock when closing, so reaching here means it's acting as a value.
                 return new PdxObject(new List<KeyValuePair<string, PdxElement>>());

            default:
                // Consider handling TokenType.Unknown more gracefully if desired
                throw new FormatException($"Unexpected token as value: {valueToken.Type} at position {valueToken.Start}");
        }
    }

    // Parses a block delimited by '{' and '}'. Determines if it's an Object or Array.
    private PdxElement ParseBlock()
    {
        ConsumeToken(); // Eat '{'
        SkipWhitespaceAndNewlines();

        if (_currentToken.Type == TokenType.CurlyClose) // Handle empty block {}
        {
            ConsumeToken(); // Eat '}'
            return new PdxObject(new List<KeyValuePair<string, PdxElement>>());
        }

        // --- Peeking Logic to determine Object vs Array ---
        bool isObject = false;
        int startPos = _lexer.CurrentPosition;
        Token startToken = _currentToken;
        try
        {
            // Try parsing the first element as if it were a value
            ParseValue();
            // Skip any whitespace after it
            SkipWhitespaceAndNewlines();
            // If the *next* token is '=', it must be an object block
            isObject = _currentToken.Type == TokenType.Equals;
        }
        catch (FormatException)
        {
            // If parsing the first element as a value failed, it *might* be a key.
            // A common case is `key=value`. If the first token is an Identifier,
            // strongly suggests it's an object.
            isObject = startToken.Type == TokenType.Identifier;
        }
        finally
        {
            // IMPORTANT: Reset lexer state regardless of peek success/failure
            _lexer.SetPosition(startPos);
            _currentToken = startToken;
        }
        // --- End Peeking Logic ---

        if (isObject)
        {
            return ParseObjectBlockContent();
        }
        else
        {
            return ParseArrayBlockContent();
        }
    }

    // Parses the content of a block assuming it's an Object ({ key=value key2=value2 ... })
    private PdxObject ParseObjectBlockContent()
    {
        var properties = new List<KeyValuePair<string, PdxElement>>();
        while (_currentToken.Type != TokenType.CurlyClose && _currentToken.Type != TokenType.EndOfFile)
        {
            SkipWhitespaceAndNewlines();
            if (_currentToken.Type == TokenType.CurlyClose) break;

            if (_currentToken.Type == TokenType.Identifier) // Expect key
            {
                string key = GetCurrentTokenSpan().ToString();
                ConsumeToken(); // Eat key identifier

                SkipWhitespaceAndNewlines();
                if (_currentToken.Type != TokenType.Equals)
                     throw new FormatException($"Expected '=' after key '{key}' inside object block, found {_currentToken.Type} at position {_currentToken.Start}");
                ConsumeToken(); // Eat '='

                PdxElement value = ParseValue();
                properties.Add(new KeyValuePair<string, PdxElement>(key, value));
            }
            // Allow comments inside objects?
            // else if (_currentToken.Type == TokenType.Comment) { ConsumeToken(); continue; }
            else
            {
                 throw new FormatException($"Expected Identifier key inside object block, found {_currentToken.Type} at position {_currentToken.Start}");
            }
        }

        if (_currentToken.Type == TokenType.CurlyClose)
            ConsumeToken(); // Eat '}'
        else
             throw new FormatException($"Expected '}}' to close object block, found {_currentToken.Type} at position {_currentToken.Start}");

        return new PdxObject(properties);
    }

        // Parses the content of a block assuming it's an Array ({ value1 value2 ... })
    private PdxArray ParseArrayBlockContent()
    {
        var items = new List<PdxElement>();
        while (_currentToken.Type != TokenType.CurlyClose && _currentToken.Type != TokenType.EndOfFile)
        {
            SkipWhitespaceAndNewlines();
            if (_currentToken.Type == TokenType.CurlyClose) break;

            // Simply parse values and add them
            items.Add(ParseValue());
            // Allow comments inside arrays?
            // if (_currentToken.Type == TokenType.Comment) { ConsumeToken(); continue; }
        }

        if (_currentToken.Type == TokenType.CurlyClose)
            ConsumeToken(); // Eat '}'
         else
             throw new FormatException($"Expected '}}' to close array block, found {_currentToken.Type} at position {_currentToken.Start}");

        return new PdxArray(items);
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