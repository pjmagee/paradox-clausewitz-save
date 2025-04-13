using System.Globalization;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a scalar float value in a Paradox save file.
/// </summary>
public readonly struct PdxFloat(float value) : IPdxScalar, IEquatable<PdxFloat>
{
    public float Value { get; } = value;
    public PdxType Type => PdxType.Float;

    public bool Equals(PdxFloat other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxFloat other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    
    public static implicit operator float(PdxFloat f) => f.Value;
    public static implicit operator PdxFloat(float f) => new PdxFloat(f);
}