namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a scalar string value in a Paradox save file.
/// </summary>
public readonly struct PdxString : IPdxScalar, IEquatable<PdxString>
{
    /// <summary>
    /// Gets the string value.
    /// </summary>
    public string Value =>  Memory.ToString();
    
    /// <summary>
    /// Gets whether the string was originally quoted in the source file.
    /// </summary>
    public bool WasQuoted { get; }
    
    /// <summary>
    /// Gets the element type.
    /// </summary>
    public PdxType Type => PdxType.String;
    
    public ReadOnlyMemory<char> Memory { get; }
    
    /// <summary>
    /// Creates a new string value.
    /// </summary>
    public PdxString(bool wasQuoted = false)
    {
        WasQuoted = wasQuoted;
    }
    
    /// <summary>
    /// Creates a new string value from a span.
    /// </summary>
    public PdxString(ReadOnlySpan<char> value, bool wasQuoted = false)
    {
        Memory = new ReadOnlyMemory<char>(value.ToArray());
        WasQuoted = wasQuoted;
    }
    
    /// <summary>
    /// Compares this string to another string for equality.
    /// </summary>
    public bool Equals(PdxString other) => Value == other.Value;
    
    /// <summary>
    /// Compares this string to another object for equality.
    /// </summary>
    public override bool Equals(object? obj) => obj is PdxString other && Equals(other);
    
    /// <summary>
    /// Gets a hash code for this string.
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();
    
    /// <summary>
    /// Gets a string representation of this string.
    /// </summary>
    public override string ToString() => WasQuoted ? $"\"{Value}\"" : Value;
    
    /// <summary>
    /// Implicitly converts a PdxString to a string.
    /// </summary>
    public static implicit operator string(PdxString str) => str.Value;
    
    /// <summary>
    /// Implicitly converts a string to a PdxString.
    /// </summary>
    public static implicit operator PdxString(string str) => new PdxString(str);
    
    public static bool operator ==(string left, PdxString right) => left.Equals(right.Value);
    public static bool operator !=(string left, PdxString right) => !left.Equals(right.Value);
    
    public static bool operator ==(PdxString left, string right) => left.Value.Equals(right);
    public static bool operator !=(PdxString left, string right) => !left.Value.Equals(right);
}