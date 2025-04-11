namespace MageeSoft.PDX.CE2;

/// <summary>
/// Represents a key-value object in a Paradox save file (V2).
/// </summary>
public class PdxObject : PdxElement, IEquatable<PdxObject>
{
    /// <summary>
    /// Gets the properties (key-value pairs) of this object.
    /// </summary>
    public List<KeyValuePair<string, PdxElement>> Properties { get; }
    
    /// <summary>
    /// Gets the type of this save element.
    /// </summary>
    public override PdxType Type => PdxType.Object;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdxObject"/> class.
    /// </summary>
    /// <param name="properties">The properties to initialize with.</param>
    public PdxObject(List<KeyValuePair<string, PdxElement>> properties)
    {
        Properties = properties;
    }
    
    // Internal constructor used by the reader to create the object efficiently,
    // converting keys from ReadOnlyMemory<char> to string only at the end.
    internal PdxObject(List<(ReadOnlyMemory<char> Key, PdxElement Value)> memoryProperties)
    {
        // Allocate the final list with the correct capacity
        Properties = new List<KeyValuePair<string, PdxElement>>(memoryProperties.Count);
        // Convert ReadOnlyMemory keys to strings only upon final object creation
        foreach (var kvp in memoryProperties)
        {
            Properties.Add(new KeyValuePair<string, PdxElement>(kvp.Key.ToString(), kvp.Value));
        }
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public bool Equals(PdxObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Properties.Count != other.Properties.Count)
            return false;

        // Compare properties in order
        for (int i = 0; i < Properties.Count; i++)
        {
            var thisKvp = Properties[i];
            var otherKvp = other.Properties[i];

            if (thisKvp.Key != otherKvp.Key)
                return false;

            // Use custom element comparison helper
            if (!PdxElementComparer.Equals(thisKvp.Value, otherKvp.Value))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object? obj) => 
        obj is PdxObject other && Equals(other);

    /// <summary>
    /// Returns a hash code for this object.
    /// </summary>
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (var kvp in Properties)
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(PdxObject? left, PdxObject? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(PdxObject? left, PdxObject? right) =>
        !(left == right);
} 