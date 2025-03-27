namespace MageeSoft.Paradox.Clausewitz.Save.Parser;

/// <summary>
/// Represents a scalar value in a save file.
/// </summary>
/// <typeparam name="T">The type of the scalar value.</typeparam>
public class Scalar<T> : SaveElement, IEquatable<Scalar<T>>
{
    /// <summary>
    /// Gets the raw text of the scalar value.
    /// </summary>
    public string RawText { get; set; }

    /// <summary>
    /// Gets the value of the scalar.
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Gets the type of the scalar value.
    /// </summary>
    public override SaveType Type => GetValueType();

    /// <summary>
    /// Initializes a new instance of the <see cref="Scalar{T}"/> class.
    /// </summary>
    /// <param name="rawText">The raw text of the scalar value.</param>
    /// <param name="value">The parsed value of the scalar.</param>
    public Scalar(string rawText, T value)
    {
        RawText = rawText;
        Value = value;
    }

    private SaveType GetValueType()
    {
        return typeof(T) switch
        {
            Type t when t == typeof(string) => SaveType.String,
            Type t when t == typeof(int) => SaveType.Int32,
            Type t when t == typeof(long) => SaveType.Int64,
            Type t when t == typeof(float) => SaveType.Float,
            Type t when t == typeof(bool) => SaveType.Bool,
            Type t when t == typeof(DateOnly) => SaveType.Date,
            Type t when t == typeof(Guid) => SaveType.Guid,
            _ => throw new ArgumentException($"Unsupported scalar type: {typeof(T).Name}")
        };
    }

    public bool Equals(Scalar<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return RawText == other.RawText && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Scalar<T>)obj);
    }

    public override int GetHashCode() => HashCode.Combine(RawText, Value);
}