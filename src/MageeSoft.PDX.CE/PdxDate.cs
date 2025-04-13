using System.Globalization;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a scalar DateTime value in a Paradox save file.
/// </summary>
public readonly struct PdxDate(DateTime value) : IPdxScalar, IEquatable<PdxDate>
{
    public DateTime Value { get; } = value;
    public PdxType Type => PdxType.Date;

    public bool Equals(PdxDate other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxDate other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString("yyyy.M.d", CultureInfo.InvariantCulture);
    
    public static implicit operator DateTime(PdxDate d) => d.Value;
    public static implicit operator PdxDate(DateTime d) => new PdxDate(d);
}