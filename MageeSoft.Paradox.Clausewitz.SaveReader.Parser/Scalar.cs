using System;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

/// <summary>
/// Represents a scalar value in a save file.
/// </summary>
/// <typeparam name="T">The type of the scalar value.</typeparam>
public class Scalar<T> : SaveElement
{
    /// <summary>
    /// Gets the raw text of the scalar value.
    /// </summary>
    public string RawText { get; }

    /// <summary>
    /// Gets the value of the scalar.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Gets the type of the scalar value.
    /// </summary>
    public override ValueType Type => GetValueType();

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

    private ValueType GetValueType()
    {
        return typeof(T) switch
        {
            Type t when t == typeof(string) => ValueType.String,
            Type t when t == typeof(int) => ValueType.Int32,
            Type t when t == typeof(long) => ValueType.Int64,
            Type t when t == typeof(float) => ValueType.Float,
            Type t when t == typeof(bool) => ValueType.Boolean,
            Type t when t == typeof(DateOnly) => ValueType.Date,
            Type t when t == typeof(Guid) => ValueType.Guid,
            _ => throw new ArgumentException($"Unsupported scalar type: {typeof(T).Name}")
        };
    }
}