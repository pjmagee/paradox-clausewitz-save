using System.Globalization;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a scalar long value in a Paradox save file.
/// </summary>
public readonly struct PdxLong(long value) : IPdxScalar, IEquatable<PdxLong>
{
    public long Value { get; } = value;
    public PdxType Type => PdxType.Int64;

    public bool Equals(PdxLong other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxLong other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    
    public static implicit operator long(PdxLong i) => i.Value;
    public static implicit operator PdxLong(long i) => new PdxLong(i);
    public string ToSaveString() => Value.ToString(CultureInfo.InvariantCulture);
}