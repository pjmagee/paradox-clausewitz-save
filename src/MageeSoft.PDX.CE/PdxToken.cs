namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a token in a Paradox save file. Stack-allocated for maximum performance.
/// </summary>
public readonly ref struct PdxToken
{
    /// <summary>The type of token.</summary>
    public readonly PdxTokenType Type;
    
    /// <summary>Start index in the source text.</summary>
    public readonly int Start;
    
    /// <summary>Length of the token.</summary>
    public readonly int Length;
    
    /// <summary>
    /// Creates a new token.
    /// </summary>
    public PdxToken(PdxTokenType type, int start, int length)
    {
        Type = type;
        Start = start;
        Length = length;
    }
} 