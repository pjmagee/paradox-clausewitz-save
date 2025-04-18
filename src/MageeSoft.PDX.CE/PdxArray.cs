namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents an array in a Paradox save file (V2).
/// </summary>
public sealed class PdxArray : IPdxElement, IEquatable<PdxArray>
{
    /// <summary>
    /// Gets the items in the array.
    /// </summary>
    public List<IPdxElement> Items { get; }
    
    /// <summary>
    /// Gets the array's type.
    /// </summary>
    public PdxType Type => PdxType.Array;
    
    /// <summary>
    /// Creates a new array with the specified items.
    /// </summary>
    public PdxArray(List<IPdxElement> items)
    {
        Items = items;
    }

    /// <summary>
    /// Creates a new empty array.
    /// </summary>
    public PdxArray() : this([])
    {
        
    }
    
    /// <summary>
    /// Compares this array to another array for equality.
    /// </summary>
    public bool Equals(PdxArray? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Items.Count != other.Items.Count) return false;
        
        for (int i = 0; i < Items.Count; i++)
        {
            if (!PdxElementEqualityComparer.Equals(Items[i], other.Items[i]))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Compares this array to another object for equality.
    /// </summary>
    public override bool Equals(object? obj) => obj is PdxArray array && Equals(array);
    
    /// <summary>
    /// Gets a hash code for this array.
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

    public override string ToString()
    {
        return new PdxSaveWriter().Write(this);
    }
} 