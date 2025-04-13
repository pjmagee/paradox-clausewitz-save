using System.Collections.Immutable;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents an array in a Paradox save file (V2).
/// </summary>
public sealed class PdxArray : IPdxElement, IEquatable<PdxArray>
{
    /// <summary>
    /// Gets the items in the array.
    /// </summary>
    public ImmutableArray<IPdxElement> Items { get; }
    
    /// <summary>
    /// Gets the array's type.
    /// </summary>
    public PdxType Type => PdxType.Array;
    
    /// <summary>
    /// Creates a new array with the specified items.
    /// </summary>
    public PdxArray(ImmutableArray<IPdxElement> items)
    {
        Items = items;
    }

    /// <summary>
    /// Creates a new empty array.
    /// </summary>
    public PdxArray() : this(ImmutableArray<IPdxElement>.Empty)
    {
        
    }
    
    /// <summary>
    /// Compares this array to another array for equality.
    /// </summary>
    public bool Equals(PdxArray? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Items.Length != other.Items.Length) return false;
        
        for (int i = 0; i < Items.Length; i++)
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
} 