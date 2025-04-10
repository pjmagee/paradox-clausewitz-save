namespace MageeSoft.PDX.CE2;

/// <summary>
/// Represents a scalar (primitive) value in a Paradox save file (V2).
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class PdxScalar<T> : PdxElement, IEquatable<PdxScalar<T>> where T : notnull
{
    /// <summary>
    /// Gets the value of this scalar.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Gets the type of this save element.
    /// </summary>
    public override PdxType Type => GetSaveType();

    /// <summary>
    /// Initializes a new instance of the <see cref="PdxScalar{T}"/> class.
    /// </summary>
    /// <param name="value">The value to store.</param>
    public PdxScalar(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Determines the SaveType based on the generic type T.
    /// </summary>
    private PdxType GetSaveType()
    {
        Type valueType = typeof(T);
        
        if (valueType == typeof(string)) return PdxType.String;
        if (valueType == typeof(bool)) return PdxType.Bool;
        if (valueType == typeof(int)) return PdxType.Int32;
        if (valueType == typeof(long)) return PdxType.Int64;
        if (valueType == typeof(float)) return PdxType.Float;
        if (valueType == typeof(double)) return PdxType.Float; // Map double to float type
        if (valueType == typeof(DateTime)) return PdxType.Date;
        if (valueType == typeof(Guid)) return PdxType.Guid;
        
        // Default fallback - this should rarely occur with proper usage
        return PdxType.String;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public bool Equals(PdxScalar<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object? obj) => 
        obj is PdxScalar<T> other && Equals(other);

    /// <summary>
    /// Returns a hash code for this object.
    /// </summary>
    public override int GetHashCode() => 
        EqualityComparer<T>.Default.GetHashCode(Value);

    public static bool operator ==(PdxScalar<T>? left, PdxScalar<T>? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(PdxScalar<T>? left, PdxScalar<T>? right) =>
        !(left == right);
} 