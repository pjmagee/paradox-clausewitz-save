namespace MageeSoft.PDX.CE2;

/// <summary>
/// Token structure for the PdxSaveReader.
/// </summary>
internal readonly struct PdxToken
{
    public PdxTokenType Type { get; }
    public int Start { get; }
    public int Length { get; }
    public string? ProcessedString { get; }

    public PdxToken(PdxTokenType type, int start, int length, string? processedString = null)
    {
        Type = type;
        Start = start;
        Length = length;
        ProcessedString = processedString;
    }
}