using System.Collections.Generic;

public class Object : Element
{
    public List<KeyValuePair<string, Element>> Properties { get; } = new();
}
