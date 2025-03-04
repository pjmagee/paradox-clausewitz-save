// Enum for the value types.
// Base class for parsed elements.
// Represents an object (with duplicate keys allowed).

public class Object : Element
{
    public List<KeyValuePair<string, Element>> Properties { get; } = new List<KeyValuePair<string, Element>>();
}
