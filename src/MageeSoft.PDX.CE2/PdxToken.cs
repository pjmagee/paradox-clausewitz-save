namespace MageeSoft.PDX.CE2;

/// <summary>
/// Token structure for the PdxSaveReader.
/// Reverted to known-good state.
/// </summary>
internal readonly struct PdxToken
{
    public PdxTokenType Type { get; }
    public int Start { get; }
    public int Length { get; }
    // String value, processed (e.g., unescaped) for string literals
    public string? ProcessedString { get; }

    // Constructor for non-string tokens or when processed string isn't needed initially
    public PdxToken(PdxTokenType type, int start, int length)
    {
        Type = type;
        Start = start;
        Length = length;
        ProcessedString = null; 
    }
    
    // Constructor specifically for string literals with processed content
    public PdxToken(PdxTokenType type, int start, int length, string processedString)
    {
        Type = type;
        Start = start;
        Length = length;
        ProcessedString = processedString;
    }
}