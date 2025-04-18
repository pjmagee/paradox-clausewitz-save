namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a scalar Guid value in a Paradox save file.
/// </summary>
public readonly struct PdxGuid(Guid value) : IPdxScalar, IEquatable<PdxGuid>
{
    public Guid Value { get; } = value;
    public PdxType Type => PdxType.Guid;

    public bool Equals(PdxGuid other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxGuid other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(PdxGuid g) => g.Value;
    public static implicit operator PdxGuid(Guid g) => new PdxGuid(g);

    public string ToSaveString() => $"\"{Value}\"";
}