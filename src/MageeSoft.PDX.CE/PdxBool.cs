namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a scalar boolean value in a Paradox save file.
/// </summary>
public readonly struct PdxBool(bool value) : IPdxScalar, IEquatable<PdxBool>
{
    public bool Value { get; } = value;
    public PdxType Type => PdxType.Bool;

    public bool Equals(PdxBool other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxBool other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value ? "yes" : "no";
    
    public static implicit operator bool(PdxBool b) => b.Value;
    public static implicit operator PdxBool(bool b) => new PdxBool(b);
    
    public string ToSaveString() => Value ? "yes" : "no";
}