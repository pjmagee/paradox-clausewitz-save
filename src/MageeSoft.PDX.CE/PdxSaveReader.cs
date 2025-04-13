using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MageeSoft.PDX.CE;

/// <summary>
/// High-performance reader for Paradox save files.
/// </summary>
public static class PdxSaveReader
{
    /// <summary>
    /// Parses a save file from string.
    /// </summary>
    public static PdxObject Read(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new PdxObject();

        return Read(text.AsSpan());
    }

    /// <summary>
    /// Parses a save file from memory.
    /// </summary>
    public static PdxObject Read(ReadOnlyMemory<char> memory)
    {
        return Read(memory.Span);
    }

    /// <summary>
    /// Parses a save file from span.
    /// </summary>
    public static PdxObject Read(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return new PdxObject();

        // Trim BOM and whitespace
        span = TrimBomAndWhitespace(span);

        if (span.IsEmpty)
            return new PdxObject();

        var parser = new Parser(span);
        return parser.ParseRoot();
    }

    /// <summary>
    /// Trims the BOM and whitespace from a span.
    /// </summary>
    private static ReadOnlySpan<char> TrimBomAndWhitespace(ReadOnlySpan<char> span)
    {
        // Skip BOM if present
        int start = 0;
        if (span.Length >= 3 && span[0] == 0xEF && span[1] == 0xBB && span[2] == 0xBF)
            start = 3;

        // Skip leading whitespace
        while (start < span.Length && char.IsWhiteSpace(span[start]))
            start++;

        if (start >= span.Length)
            return ReadOnlySpan<char>.Empty;

        // Skip trailing whitespace
        int end = span.Length - 1;
        while (end > start && char.IsWhiteSpace(span[end]))
            end--;

        return span.Slice(start, end - start + 1);
    }

    /// <summary>
    /// Internal ref struct parser that works on spans.
    /// </summary>
    private ref struct Parser
    {
        readonly static ReadOnlyMemory<char> YesBool = "yes".AsMemory();
        readonly static ReadOnlyMemory<char> NoBool = "no".AsMemory();

        private readonly ReadOnlySpan<char> _inputSpan;
        private PdxLexer _lexer;
        private PdxToken _currentToken;

        public Parser(ReadOnlySpan<char> inputSpan)
        {
            _inputSpan = inputSpan;
            _lexer = new PdxLexer(inputSpan);
            _currentToken = _lexer.NextToken();
        }

        /// <summary>
        /// Advances to the next token.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ConsumeToken()
        {
            _currentToken = _lexer.NextToken();
        }

        /// <summary>
        /// Skips whitespace and newline tokens.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipWhitespaceAndNewlines()
        {
            while (_currentToken.Type is PdxTokenType.Whitespace or PdxTokenType.NewLine)
                ConsumeToken();
        }

        /// <summary>
        /// Gets the span for the current token.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<char> GetCurrentTokenSpan()
        {
            return _inputSpan.Slice(_currentToken.Start, _currentToken.Length);
        }

        /// <summary>
        /// Parses the root object.
        /// </summary>
        public PdxObject ParseRoot()
        {
            var propertiesBuilder = ImmutableArray.CreateBuilder<KeyValuePair<PdxString, IPdxElement>>();

            while (_currentToken.Type != PdxTokenType.EndOfFile)
            {
                SkipWhitespaceAndNewlines();

                if (_currentToken.Type == PdxTokenType.EndOfFile)
                    break;

                if (_currentToken.Type is PdxTokenType.Identifier or PdxTokenType.StringLiteral)
                {
                    // Get the key with proper quoting information
                    bool wasQuoted = _currentToken.Type == PdxTokenType.StringLiteral;
                    ReadOnlySpan<char> keySpan;

                    if (wasQuoted)
                    {
                        // Handle quoted keys - remove the quotes
                        var span = GetCurrentTokenSpan();

                        // Remove quotes from string literals
                        if (span.Length >= 2 && span[0] == '"' && span[^1] == '"')
                            keySpan = span.Slice(1, span.Length - 2);
                        else
                            keySpan = span;
                    }
                    else
                    {
                        // Regular identifier
                        keySpan = GetCurrentTokenSpan();
                    }

                    // Create a PdxString for the key
                    var key = new PdxString(keySpan, wasQuoted);

                    ConsumeToken();

                    SkipWhitespaceAndNewlines();

                    // Skip equals sign if present
                    if (_currentToken.Type == PdxTokenType.Equals)
                        ConsumeToken();

                    // Parse the value
                    var value = ParseValue();
                    propertiesBuilder.Add(new KeyValuePair<PdxString, IPdxElement>(key, value));
                }
                else
                {
                    // Skip unexpected tokens
                    ConsumeToken();
                }
            }

            return new PdxObject(propertiesBuilder.ToImmutable());
        }

        /// <summary>
        /// Parses a value.
        /// </summary>
        private IPdxElement ParseValue()
        {
            SkipWhitespaceAndNewlines();

            switch (_currentToken.Type)
            {
                case PdxTokenType.EndOfFile:
                    return new PdxString(string.Empty);

                case PdxTokenType.CurlyOpen:
                    return ParseBlock();

                case PdxTokenType.StringLiteral:
                {
                    ReadOnlySpan<char> span = GetCurrentTokenSpan();
                    ConsumeToken();

                    // Handle quoted string
                    //string str = span.ToString();
                    bool wasQuoted = true;

                    // Remove quotes
                    if (span.Length >= 2 && span[0] == '"' && span[span.Length - 1] == '"')
                        span = span.Slice(1, span.Length - 2);

                    // Try parsing as specialized types
                    if (Guid.TryParse(span, out var guid))
                        return new PdxGuid(guid);

                    if (TryParseDate(span, out var date))
                        return new PdxDate(date);

                    return new PdxString(span, wasQuoted);
                }

                case PdxTokenType.Identifier:
                {
                    var span = GetCurrentTokenSpan();
                    ConsumeToken();

                    // Handle yes/no values
                    if (span.Equals(YesBool.Span, StringComparison.OrdinalIgnoreCase))
                        return new PdxBool(true);

                    if (span.Equals(NoBool.Span, StringComparison.OrdinalIgnoreCase))
                        return new PdxBool(false);

                    // identifiers = not quoted
                    return new PdxString(span, wasQuoted: false);
                }

                case PdxTokenType.NumberLiteral:
                {
                    var span = GetCurrentTokenSpan();
                    ConsumeToken();

                    // Check if it contains decimal point or exponent
                    bool hasDecimal = false;
                    bool hasExponent = false;

                    for (int i = 0; i < span.Length; i++)
                    {
                        if (span[i] == '.')
                            hasDecimal = true;
                        else if (span[i] == 'e' || span[i] == 'E')
                            hasExponent = true;
                    }

                    // Parse as appropriate numeric type
                    if (hasDecimal || hasExponent)
                    {
                        if (float.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                            return new PdxFloat(f);
                    }
                    else
                    {
                        if (int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                            return new PdxInt(i);

                        if (long.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
                            return new PdxLong(l);
                    }

                    // Fallback to string if parsing fails
                    return new PdxString(span, wasQuoted: false);
                }

                default:
                    // Handle unexpected tokens
                    var defaultSpan = GetCurrentTokenSpan();
                    ConsumeToken();
                    return new PdxString(defaultSpan, wasQuoted: false);
            }
        }

        /// <summary>
        /// Parses a block (object or array).
        /// </summary>
        private IPdxElement ParseBlock()
        {
            ConsumeToken(); // Skip opening brace
            SkipWhitespaceAndNewlines();

            // Handle empty block
            if (_currentToken.Type == PdxTokenType.CurlyClose)
            {
                ConsumeToken(); // Skip closing brace
                return new PdxObject();
            }

            var propertiesBuilder = ImmutableArray.CreateBuilder<KeyValuePair<PdxString, IPdxElement>>();
            var itemsBuilder = ImmutableArray.CreateBuilder<IPdxElement>();
            bool isObject = false;
            bool isArray = false;

            while (_currentToken.Type != PdxTokenType.CurlyClose && _currentToken.Type != PdxTokenType.EndOfFile)
            {
                SkipWhitespaceAndNewlines();

                if (_currentToken.Type == PdxTokenType.CurlyClose || _currentToken.Type == PdxTokenType.EndOfFile)
                    break;

                // Look ahead to see if we have a property (key=value) or a simple value
                bool hasEquals = PeekForEquals();

                if (hasEquals)
                {
                    // This is a property
                    isObject = true;

                    // Get the key with proper quoting information
                    bool wasQuoted = _currentToken.Type == PdxTokenType.StringLiteral;
                    ReadOnlySpan<char> keySpan;

                    if (wasQuoted)
                    {
                        // Handle quoted keys - remove the quotes
                        var span = GetCurrentTokenSpan();

                        // Remove quotes from string literals
                        if (span.Length >= 2 && span[0] == '"' && span[^1] == '"')
                            keySpan = span.Slice(1, span.Length - 2);
                        else
                            keySpan = span;
                    }
                    else
                    {
                        // Regular identifier
                        keySpan = GetCurrentTokenSpan();
                    }

                    // Create a PdxString for the key
                    var key = new PdxString(keySpan, wasQuoted);

                    ConsumeToken();

                    SkipWhitespaceAndNewlines();

                    // Skip equals sign
                    ConsumeToken();

                    // Parse the value
                    var value = ParseValue();
                    propertiesBuilder.Add(new KeyValuePair<PdxString, IPdxElement>(key, value));
                }
                else
                {
                    // This is a simple value
                    isArray = true;

                    // Parse the value
                    var value = ParseValue();
                    itemsBuilder.Add(value);
                }
            }

            // Skip closing brace
            if (_currentToken.Type == PdxTokenType.CurlyClose)
                ConsumeToken();

            // Handle mixed content
            if (isObject && isArray)
            {
                // If we have both properties and items, add the items as indexed properties
                for (int i = 0; i < itemsBuilder.Count; i++)
                {
                    // Use a PdxString for the index
                    var indexKey = new PdxString(i.ToString(), wasQuoted: false);
                    propertiesBuilder.Add(new KeyValuePair<PdxString, IPdxElement>(indexKey, itemsBuilder[i]));
                }

                return new PdxObject(propertiesBuilder.ToImmutable());
            }
            else if (isObject)
            {
                // If we have only properties, return an object
                return new PdxObject(propertiesBuilder.ToImmutable());
            }
            else
            {
                // If we have only items, return an array
                return new PdxArray(itemsBuilder.ToImmutable());
            }
        }

        /// <summary>
        /// Looks ahead to see if the current token is followed by an equals sign.
        /// </summary>
        private bool PeekForEquals()
        {
            if (_currentToken.Type is not (PdxTokenType.Identifier or PdxTokenType.StringLiteral or PdxTokenType.NumberLiteral))
                return false;

            // Save current state
            int savedPosition = _lexer.Position;
            PdxToken savedToken = _currentToken;

            // Skip current token
            ConsumeToken();

            // Skip whitespace and newlines
            SkipWhitespaceAndNewlines();

            // Check if we have an equals sign
            bool hasEquals = _currentToken.Type == PdxTokenType.Equals;

            // Restore state
            _lexer.SetPosition(savedPosition);
            _currentToken = savedToken;

            return hasEquals;
        }
    }

    readonly static string[] Formats = ["yyyy.M.d", "yyyy.MM.dd", "yyyy.MM.d", "yyyy.M.dd"];

    /// <summary>
    /// Tries to parse a date from a string.
    /// </summary>
    private static bool TryParseDate(ReadOnlySpan<char> span, out DateTime date)
    {
        try
        {
            // Try to parse YYYY.MM.DD format
            if (span.Length >= 10 && span[4] == '.' && span[7] == '.')
            {
                if (int.TryParse(span.Slice(0, 4), out int year) &&
                    int.TryParse(span.Slice(5, 2), out int month) &&
                    int.TryParse(span.Slice(8, 2), out int day) &&
                    year > 0 && month > 0 && month <= 12 && day > 0 && day <= DateTime.DaysInMonth(year, month))
                {
                    date = new DateTime(year, month, day);
                    return true;
                }
            }

            // Try to parse YYYY.M.D format with standard formats
            return DateTime.TryParseExact(span, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
        catch
        {
            date = DateTime.MinValue;
            return false;
        }
    }
}