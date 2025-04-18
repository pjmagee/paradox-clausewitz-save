using System.Globalization;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents a scalar DateOnly value in a Paradox save file.
/// </summary>
public readonly struct PdxDate(DateOnly value) : IPdxScalar, IEquatable<PdxDate>
{
    public DateOnly Value { get; } = value;
    public PdxType Type => PdxType.Date;

    public bool Equals(PdxDate other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxDate other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static implicit operator DateOnly(PdxDate d) => d.Value;
    public static implicit operator PdxDate(DateOnly d) => new PdxDate(d);
    
    public string ToSaveString() => Value.ToString(CultureInfo.InvariantCulture);
}