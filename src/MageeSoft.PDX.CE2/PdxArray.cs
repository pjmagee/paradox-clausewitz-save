namespace MageeSoft.PDX.CE2;

/// <summary>
/// Represents an array in a Paradox save file (V2).
/// </summary>
public class PdxArray : PdxElement, IEquatable<PdxArray>
{
    /// <summary>
    /// Gets the items in this array.
    /// </summary>
    public List<PdxElement> Items { get; }
    
    /// <summary>
    /// Gets the type of this save element.
    /// </summary>
    public override PdxType Type => PdxType.Array;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdxArray"/> class.
    /// </summary>
    /// <param name="items">The items to initialize with.</param>
    public PdxArray(List<PdxElement> items)
    {
        Items = items;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public bool Equals(PdxArray? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Items.Count != other.Items.Count)
            return false;

        // Compare array items in order
        for (int i = 0; i < Items.Count; i++)
        {
            if (!PdxElementComparer.Equals(Items[i], other.Items[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object? obj) => 
        obj is PdxArray other && Equals(other);

    /// <summary>
    /// Returns a hash code for this object.
    /// </summary>
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (var item in Items)
        {
            hash.Add(item);
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(PdxArray? left, PdxArray? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(PdxArray? left, PdxArray? right) =>
        !(left == right);
} 