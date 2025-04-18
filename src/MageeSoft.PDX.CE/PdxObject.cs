namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents an object in a Paradox save file (V2).
/// </summary>
public class PdxObject : IPdxElement, IEquatable<PdxObject>
{
    /// <summary>
    /// Gets the properties in the object.
    /// </summary>
    public List<KeyValuePair<IPdxScalar, IPdxElement>> Properties { get; }
    
    public PdxType Type => PdxType.Object;

    /// <summary>
    /// Creates a new object with the specified properties.
    /// </summary>
    public PdxObject(List<KeyValuePair<IPdxScalar, IPdxElement>> properties)
    {
        Properties = properties;
    }

    /// <summary>
    /// Creates a new empty object.
    /// </summary>
    public PdxObject() : this([])
    {

    }

    public override string ToString()
    {
        return new PdxSaveWriter().Write(this);
    }

    /// <summary>
    /// Compares this object to another object for equality.
    /// </summary>
    public bool Equals(PdxObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Properties.Count != other.Properties.Count) return false;

        for (int i = 0; i < Properties.Count; i++)
        {
            var thisKvp = Properties[i];
            var otherKvp = other.Properties[i];
            if (!PdxElementEqualityComparer.Equals(thisKvp.Key, otherKvp.Key)) return false;
            if (!PdxElementEqualityComparer.Equals(thisKvp.Value, otherKvp.Value)) return false;
        }

        return true;
    }

    /// <summary>
    /// Compares this object to another object for equality.
    /// </summary>
    public override bool Equals(object? obj) => obj is PdxObject other && Equals(other);

    /// <summary>
    /// Gets a hash code for this object.
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
}