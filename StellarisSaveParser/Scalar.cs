
/// <summary>
/// Represents a scalar value
/// </summary>
public class Scalar : Element
{
    /// <summary>
    /// The raw text of the scalar.
    /// </summary>
    public string RawText { get; }
    /// <summary>
    /// The type of the scalar.
    /// </summary>
    public ValueType ValueType { get; }
    /// <summary>
    /// The value of the scalar.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Scalar"/> class.
    /// </summary>
    /// <param name="rawText">The raw text of the scalar.</param>
    /// <param name="valueType">The type of the scalar.</param>
    /// <param name="value">The value of the scalar.</param>
    public Scalar(string rawText, ValueType valueType, object value)
    {
        RawText = rawText;
        ValueType = valueType;
        Value = value;
    }
}
