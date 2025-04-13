using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents an object in a Paradox save file (V2).
/// </summary>
public sealed class PdxObject : IPdxElement, IEquatable<PdxObject>
{
    /// <summary>
    /// Gets the properties in the object.
    /// </summary>
    public ImmutableArray<KeyValuePair<PdxString, IPdxElement>> Properties { get; }

    /// <summary>
    /// Gets the object's type.
    /// </summary>
    public PdxType Type => PdxType.Object;

    /// <summary>
    /// Creates a new object with the specified properties.
    /// </summary>
    public PdxObject(ImmutableArray<KeyValuePair<PdxString, IPdxElement>> properties)
    {
        Properties = properties;
    }

    /// <summary>
    /// Creates a new empty object.
    /// </summary>
    public PdxObject() : this(ImmutableArray<KeyValuePair<PdxString, IPdxElement>>.Empty)
    {

    }

    public string? ToSaveString()
    {
        return new PdxSaveWriter().Write(this);
    }

    /// <summary>
    /// Tries to get a property value by key.
    /// </summary>
    public bool TryGetValue(string key, [NotNullWhen(true)] out IPdxElement? value)
    {
        foreach (var prop in Properties)
        {
            if (string.Equals(prop.Key.Value, key, StringComparison.Ordinal))
            {
                value = prop.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Tries to get a property value by key and type.
    /// </summary>
    public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value) where T : class, IPdxElement
    {
        if (TryGetValue(key, out var element) && element is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Tries to get a string value directly.
    /// </summary>
    public bool TryGetString(string key, out string? value)
    {
        if (TryGetValue(key, out var element) && element is PdxString str)
        {
            value = str.Value;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Tries to get a boolean value directly.
    /// </summary>
    public bool TryGetBool(string key, out bool value)
    {
        if (TryGetValue(key, out var element) && element is PdxBool b)
        {
            value = b.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get an integer value directly.
    /// </summary>
    public bool TryGetInt(string key, out int value)
    {
        if (TryGetValue(key, out var element) && element is PdxInt i)
        {
            value = i.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get a long value directly.
    /// </summary>
    public bool TryGetLong(string key, out long value)
    {
        if (TryGetValue(key, out var element) && element is PdxLong l)
        {
            value = l.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get a float value directly.
    /// </summary>
    public bool TryGetFloat(string key, out float value)
    {
        if (TryGetValue(key, out var element) && element is PdxFloat f)
        {
            value = f.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get a date value directly.
    /// </summary>
    public bool TryGetDate(string key, out DateTime value)
    {
        if (TryGetValue(key, out var element) && element is PdxDate d)
        {
            value = d.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get a guid value directly.
    /// </summary>
    public bool TryGetGuid(string key, out Guid value)
    {
        if (TryGetValue(key, out var element) && element is PdxGuid g)
        {
            value = g.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Compares this object to another object for equality.
    /// </summary>
    public bool Equals(PdxObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Properties.Length != other.Properties.Length) return false;

        for (int i = 0; i < Properties.Length; i++)
        {
            var thisKvp = Properties[i];
            var otherKvp = other.Properties[i];

            if (!thisKvp.Key.Equals(otherKvp.Key)) return false;
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