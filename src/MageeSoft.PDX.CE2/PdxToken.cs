namespace MageeSoft.PDX.CE2;

/// <summary>
/// Token structure for the PdxSaveReader.
/// </summary>
internal readonly struct PdxToken
{
    public PdxTokenType Type { get; }
    public int Start { get; }
    public int Length { get; }
    
    // Memory range for storing string literal content without quotes/escapes
    // This is null for non-string tokens
    public ReadOnlyMemory<char>? ValueMemory { get; }

    public PdxToken(PdxTokenType type, int start, int length, ReadOnlyMemory<char>? valueMemory = null)
    {
        Type = type;
        Start = start;
        Length = length;
        ValueMemory = valueMemory;
    }
}