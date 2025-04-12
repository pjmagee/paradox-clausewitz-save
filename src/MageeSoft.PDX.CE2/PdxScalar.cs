using System.Globalization;
using System.Runtime.CompilerServices;

namespace MageeSoft.PDX.CE2;

/// <summary>
/// Base interface for scalar values
/// </summary>
public interface IPdxScalar : IPdxElement { }

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
    public override string ToString() => Value;
    
    /// <summary>
    /// Implicitly converts a PdxString to a string.
    /// </summary>
    public static implicit operator string(PdxString str) => str.Value;
    
    /// <summary>
    /// Implicitly converts a string to a PdxString.
    /// </summary>
    public static implicit operator PdxString(string str) => new PdxString(str);
}

/// <summary>
/// Represents a scalar boolean value in a Paradox save file.
/// </summary>
public readonly struct PdxBool : IPdxScalar, IEquatable<PdxBool>
{
    public bool Value { get; }
    public PdxType Type => PdxType.Bool;
    
    public PdxBool(bool value)
    {
        Value = value;
    }
    
    public bool Equals(PdxBool other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxBool other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value ? "yes" : "no";
    
    public static implicit operator bool(PdxBool b) => b.Value;
    public static implicit operator PdxBool(bool b) => new PdxBool(b);
}

/// <summary>
/// Represents a scalar integer value in a Paradox save file.
/// </summary>
public readonly struct PdxInt : IPdxScalar, IEquatable<PdxInt>
{
    public int Value { get; }
    public PdxType Type => PdxType.Int32;
    
    public PdxInt(int value)
    {
        Value = value;
    }
    
    public bool Equals(PdxInt other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxInt other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    
    public static implicit operator int(PdxInt i) => i.Value;
    public static implicit operator PdxInt(int i) => new PdxInt(i);
}

/// <summary>
/// Represents a scalar long value in a Paradox save file.
/// </summary>
public readonly struct PdxLong : IPdxScalar, IEquatable<PdxLong>
{
    public long Value { get; }
    public PdxType Type => PdxType.Int64;
    
    public PdxLong(long value)
    {
        Value = value;
    }
    
    public bool Equals(PdxLong other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxLong other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    
    public static implicit operator long(PdxLong i) => i.Value;
    public static implicit operator PdxLong(long i) => new PdxLong(i);
}

/// <summary>
/// Represents a scalar float value in a Paradox save file.
/// </summary>
public readonly struct PdxFloat : IPdxScalar, IEquatable<PdxFloat>
{
    public float Value { get; }
    public PdxType Type => PdxType.Float;
    
    public PdxFloat(float value)
    {
        Value = value;
    }
    
    public bool Equals(PdxFloat other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxFloat other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    
    public static implicit operator float(PdxFloat f) => f.Value;
    public static implicit operator PdxFloat(float f) => new PdxFloat(f);
}

/// <summary>
/// Represents a scalar DateTime value in a Paradox save file.
/// </summary>
public readonly struct PdxDate : IPdxScalar, IEquatable<PdxDate>
{
    public DateTime Value { get; }
    public PdxType Type => PdxType.Date;
    
    public PdxDate(DateTime value)
    {
        Value = value;
    }
    
    public bool Equals(PdxDate other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxDate other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString("yyyy.M.d", CultureInfo.InvariantCulture);
    
    public static implicit operator DateTime(PdxDate d) => d.Value;
    public static implicit operator PdxDate(DateTime d) => new PdxDate(d);
}

/// <summary>
/// Represents a scalar Guid value in a Paradox save file.
/// </summary>
public readonly struct PdxGuid : IPdxScalar, IEquatable<PdxGuid>
{
    public Guid Value { get; }
    public PdxType Type => PdxType.Guid;
    
    public PdxGuid(Guid value)
    {
        Value = value;
    }
    
    public bool Equals(PdxGuid other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is PdxGuid other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(PdxGuid g) => g.Value;
    public static implicit operator PdxGuid(Guid g) => new PdxGuid(g);
} 