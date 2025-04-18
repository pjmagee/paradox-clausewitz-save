using System.Globalization;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a scalar integer value in a Paradox save file.
/// </summary>
public readonly struct PdxInt(int value) : IPdxScalar, IEquatable<PdxInt>
{
    public int Value { get; } = value;
    public PdxType Type => PdxType.Int32;

    public bool Equals(PdxInt other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxInt other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    
    public static implicit operator int(PdxInt i) => i.Value;
    public static implicit operator PdxInt(int i) => new PdxInt(i);
    public string ToSaveString() => Value.ToString(CultureInfo.InvariantCulture);
}