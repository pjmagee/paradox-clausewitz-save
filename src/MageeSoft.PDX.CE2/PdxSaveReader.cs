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

    // Helper to get the ReadOnlyMemory<char> corresponding to the current token's span
    private ReadOnlyMemory<char> GetCurrentTokenMemory() => _inputMemory.Slice(_currentToken.Start, _currentToken.Length);

    // Helper to get the string representation of the current token's span
    private string GetCurrentTokenText() => _inputMemory.Span.Slice(_currentToken.Start, _currentToken.Length).ToString();

    // Top-level parse: a series of key=value pairs.
    private PdxObject ParseInternal()
    {
        List<(ReadOnlyMemory<char> Key, PdxElement Value)> tempItems = new();

        while (_currentToken.Type != PdxTokenType.EndOfFile)
        {
            SkipWhitespaceAndNewlines();
            if (_currentToken.Type == PdxTokenType.EndOfFile) break;

            ReadOnlyMemory<char> keyMemory;
            if (_currentToken.Type == PdxTokenType.Identifier)
            {
                keyMemory = GetCurrentTokenMemory();
                ConsumeToken(); 
            }
            else if (_currentToken.Type == PdxTokenType.StringLiteral)
            {
                // Use ProcessedString for the key content, but store as memory
                string keyString = _currentToken.ProcessedString ?? GetCurrentTokenText();
                keyMemory = keyString.AsMemory(); 
                ConsumeToken(); 
            }
            else
            {
                ConsumeToken(); continue; 
            }

            SkipWhitespaceAndNewlines();
            if (_currentToken.Type == PdxTokenType.Equals) ConsumeToken(); 
            else throw new FormatException($"Expected '=' after key \"{keyMemory.ToString()}\" at position {_currentToken.Start}");

            PdxElement value = ParseValue();
            tempItems.Add((keyMemory, value)); 
        }
        // Use internal constructor
        return new PdxObject(tempItems);
    }

    // Parses a value (block or primitive) and infers its type.
    private PdxElement ParseValue()
    {
        SkipWhitespaceAndNewlines();
        if (_currentToken.Type == PdxTokenType.EndOfFile) return new PdxScalar<string>("");
        if (_currentToken.Type == PdxTokenType.CurlyOpen) return ParseBlock();
        if (_currentToken.Type == PdxTokenType.CurlyClose) return new PdxObject(new List<KeyValuePair<string, PdxElement>>());

        if (_currentToken.Type == PdxTokenType.StringLiteral)
        {
            string stringValue = _currentToken.ProcessedString ?? GetCurrentTokenText();
            ConsumeToken(); 
            SkipWhitespaceAndNewlines();
            if (Guid.TryParse(stringValue, out Guid guid)) return new PdxScalar<Guid>(guid);
            if (TryParseDate(stringValue, out DateTime date)) return new PdxScalar<DateTime>(date);
            return new PdxScalar<string>(stringValue);
        }

        if (_currentToken.Type == PdxTokenType.NumberLiteral || _currentToken.Type == PdxTokenType.Identifier)
        {
            // Use span directly for number/identifier parsing
            ReadOnlySpan<char> tokenSpan = _inputMemory.Span.Slice(_currentToken.Start, _currentToken.Length);
            PdxTokenType origType = _currentToken.Type;
            ConsumeToken(); 
            SkipWhitespaceAndNewlines(); 

            if (origType == PdxTokenType.Identifier)
            {
                if (tokenSpan.Equals("yes".AsSpan(), StringComparison.OrdinalIgnoreCase)) return new PdxScalar<bool>(true);
                if (tokenSpan.Equals("no".AsSpan(), StringComparison.OrdinalIgnoreCase)) return new PdxScalar<bool>(false);
                return new PdxScalar<string>(tokenSpan.ToString());
            }
            else // NumberLiteral
            {
                if (tokenSpan.Contains('.') || tokenSpan.Contains('e') || tokenSpan.Contains('E')) 
                {
                    if (float.TryParse(tokenSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) return new PdxScalar<float>(f);
                }
                else 
                {
                    if (int.TryParse(tokenSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out int i32)) return new PdxScalar<int>(i32);
                    if (long.TryParse(tokenSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out long i64)) return new PdxScalar<long>(i64);
                }
                return new PdxScalar<string>(tokenSpan.ToString()); // Fallback if parsing fails
            }
        }
        else if (_currentToken.Type == PdxTokenType.Equals)
        {
            ReadOnlySpan<char> tokenSpan = GetCurrentTokenSpan(); // Use span
            ConsumeToken();
            SkipWhitespaceAndNewlines(); 
            return new PdxScalar<string>(tokenSpan.ToString()); // Convert only for scalar
        }

        // Default fallback
        ReadOnlySpan<char> defaultSpan = GetCurrentTokenSpan(); // Use span
        ConsumeToken();
        SkipWhitespaceAndNewlines(); 
        return new PdxScalar<string>(defaultSpan.ToString()); // Convert only for scalar
    }

    // Parses a block delimited by '{' and '}'. Determines if it's an Object or Array.
    private PdxElement ParseBlock()
    {
        ConsumeToken(); // Eat '{'
        SkipWhitespaceAndNewlines();
        if (_currentToken.Type == PdxTokenType.CurlyClose) { ConsumeToken(); return new PdxObject(new List<KeyValuePair<string, PdxElement>>()); } 

        bool isProperty = false;
        if (_currentToken.Type == PdxTokenType.Identifier || _currentToken.Type == PdxTokenType.NumberLiteral || _currentToken.Type == PdxTokenType.StringLiteral)
        {
            int savedPosition = _lexer.CurrentPosition;
            PdxToken savedToken = _currentToken;
            PdxToken next = _lexer.NextToken(); 
            while (next.Type == PdxTokenType.Whitespace || next.Type == PdxTokenType.NewLine) next = _lexer.NextToken(); 
            if (next.Type == PdxTokenType.Equals) isProperty = true;
            _lexer.SetPosition(savedPosition);
            _currentToken = savedToken;
        }

        var tempProperties = new List<(ReadOnlyMemory<char> Key, PdxElement Value)>();
        var values = new List<PdxElement>();

        if (isProperty)
        {
            while (_currentToken.Type != PdxTokenType.CurlyClose && _currentToken.Type != PdxTokenType.EndOfFile)
            {
                SkipWhitespaceAndNewlines();
                if (_currentToken.Type == PdxTokenType.CurlyClose || _currentToken.Type == PdxTokenType.EndOfFile) break;

                ReadOnlyMemory<char> keyMemory;
                if (_currentToken.Type == PdxTokenType.Identifier || _currentToken.Type == PdxTokenType.NumberLiteral)
                {
                    keyMemory = GetCurrentTokenMemory();
                    ConsumeToken();
                }
                else if (_currentToken.Type == PdxTokenType.StringLiteral)
                {
                    // Use ProcessedString, store as memory
                    string keyString = _currentToken.ProcessedString ?? GetCurrentTokenText();
                    keyMemory = keyString.AsMemory();
                    ConsumeToken();
                }
                else throw new FormatException($"Unexpected token type for key: {_currentToken.Type} at position {_currentToken.Start}");

                SkipWhitespaceAndNewlines();
                if (_currentToken.Type != PdxTokenType.Equals) throw new FormatException($"Expected '=' after key \"{keyMemory.ToString()}\" inside block at position {_currentToken.Start}");
                ConsumeToken(); // Consume '='
                
                PdxElement value = ParseValue();
                tempProperties.Add((keyMemory, value)); 
            }
        }
        else
        {
            while (_currentToken.Type != PdxTokenType.CurlyClose && _currentToken.Type != PdxTokenType.EndOfFile)
            {
                values.Add(ParseValue());
                SkipWhitespaceAndNewlines(); 
            }
        }
        
        if (_currentToken.Type == PdxTokenType.CurlyClose) ConsumeToken(); 

        if (tempProperties.Count > 0 && values.Count > 0)
        {
            for (int i = 0; i < values.Count; i++)
            {
                tempProperties.Add((new ReadOnlyMemory<char>(new[] { (char)('0' + i) }), values[i]));
            }
            return new PdxObject(tempProperties); // Use internal constructor
        }
        else if (tempProperties.Count > 0)
        {
            return new PdxObject(tempProperties); // Use internal constructor
        }
        else 
        {
            return new PdxArray(values);
        }
    }

    // Helper to reconstruct escaped string if ValueMemory wasn't usable
    private string ReconstructEscapedString(ReadOnlySpan<char> rawSpanWithQuotes)
    {
        // Basic validation: must start and end with quotes for StringLiteral span
        if (rawSpanWithQuotes.Length < 2 || rawSpanWithQuotes[0] != '"' || rawSpanWithQuotes[^1] != '"')
        {
             // This case shouldn't typically be hit if the input is from a StringLiteral token span
             return rawSpanWithQuotes.ToString(); 
        }
        
        // Get the content span excluding the quotes
        var contentSpan = rawSpanWithQuotes[1..^1];
        
        // Optimization: If no backslash exists, no escapes are present
        if (!contentSpan.Contains('\\')) return contentSpan.ToString();

        // Allocate StringBuilder only if escapes are confirmed
        StringBuilder sb = new StringBuilder(contentSpan.Length); // Initial capacity
        for(int i = 0; i < contentSpan.Length; i++)
        {
            if (contentSpan[i] == '\\' && i + 1 < contentSpan.Length)
            {
                i++; // Move to the escaped character
                // Handle specific escapes recognised by Paradox format
                if (contentSpan[i] == '"') sb.Append('"');       // Escaped quote
                else if (contentSpan[i] == '\\') sb.Append('\\');   // Escaped backslash
                // else if (contentSpan[i] == 'n') sb.Append('\n'); // Example: Add newline if needed
                // else if (contentSpan[i] == 't') sb.Append('\t'); // Example: Add tab if needed
                else sb.Append(contentSpan[i]); // Treat other escaped chars literally for now
            }
            else
            {
                sb.Append(contentSpan[i]); // Append non-escaped character
            }
        }
        return sb.ToString();
    }

    // Helper for parsing GUID using span to avoid string allocation
    private static bool TryParseGuid(ReadOnlySpan<char> span, out Guid result)
    {
        // Guid.TryParse supports ReadOnlySpan<char>
        return Guid.TryParse(span, out result);
    }

    // Helper for parsing multiple date formats efficiently using spans
    private static bool TryParseDate(ReadOnlySpan<char> span, out DateTime date)
    {
        // Use InvariantCulture to ensure consistent parsing regardless of system locale
        string[] formats = { "yyyy.M.d", "yyyy.MM.dd", "yyyy.MM.d", "yyyy.M.dd" };
        // DateTime.TryParseExact supports ReadOnlySpan<char>
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(span, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return true;
        }
        date = default;
        return false;
    }
    
    // Overload for string parsing needed for reconstructed escaped strings
    private static bool TryParseDate(string s, out DateTime date)
    {
        string[] formats = { "yyyy.M.d", "yyyy.MM.dd", "yyyy.MM.d", "yyyy.M.dd" };
        return DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
} 