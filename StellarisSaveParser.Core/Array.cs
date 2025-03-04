// Enum for the value types.
// Base class for parsed elements.
// Represents an object (with duplicate keys allowed).
// Represents an array.
public class Array : Element
{
    public List<Element> Items { get; } = new List<Element>();
}
