// Enum for the value types.
// Base class for parsed elements.
// Represents an object (with duplicate keys allowed).
// Represents a primitive value along with its type and parsed value.
public class Scalar : Element
{
    public string RawText { get; }
    public ValueType ValueType { get; }
    public object Value { get; }

    public Scalar(string rawText, ValueType valueType, object value)
    {
        RawText = rawText;
        ValueType = valueType;
        Value = value;
    }
}
