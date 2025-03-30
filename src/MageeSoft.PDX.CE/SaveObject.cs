using System;
using System.Collections.Generic;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Represents an object in a save file.
/// </summary>
public class SaveObject : SaveElement, IEquatable<SaveObject>
{
    public List<KeyValuePair<string, SaveElement>> Properties { get; }
    public override SaveType Type => SaveType.Object;

    public SaveObject(List<KeyValuePair<string, SaveElement>> properties)
    {
        Properties = properties;
    }

    public bool Equals(SaveObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Properties.Count != other.Properties.Count)
            return false;

        // Compare properties in order since they're immutable arrays
        for (int i = 0; i < Properties.Count; i++)
        {
            var thisKvp = Properties[i];
            var otherKvp = other.Properties[i];

            if (thisKvp.Key != otherKvp.Key)
                return false;

            if (!ElementEquals(thisKvp.Value, otherKvp.Value))
                return false;
        }

        return true;
    }

    private static bool ElementEquals(SaveElement? a, SaveElement? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        if (a.Type != b.Type) return false;

        return a.Type switch
        {
            SaveType.Object => ((SaveObject)a).Equals((SaveObject)b),
            SaveType.Array => ((SaveArray)a).Equals((SaveArray)b),
            SaveType.String => ((Scalar<string>)a).Equals((Scalar<string>)b),
            SaveType.Identifier => ((Scalar<string>)a).Equals((Scalar<string>)b),
            SaveType.Bool => ((Scalar<bool>)a).Equals((Scalar<bool>)b),
            SaveType.Float => ((Scalar<float>)a).Equals((Scalar<float>)b),
            SaveType.Int32 => ((Scalar<int>)a).Equals((Scalar<int>)b),
            SaveType.Int64 => ((Scalar<long>)a).Equals((Scalar<long>)b),
            SaveType.Date => ((Scalar<DateTime>)a).Equals((Scalar<DateTime>)b),
            SaveType.Guid => ((Scalar<Guid>)a).Equals((Scalar<Guid>)b),
            _ => false
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is SaveObject other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = 0;
        // var hash = new HashCode();
        foreach (var kvp in Properties)
        {
            hash ^= hash ^= kvp.Key.GetHashCode();
            hash ^= kvp.Value.GetHashCode();
            //hash.Add(kvp.Key);
            //hash.Add(kvp.Value);
        }
        //return hash.ToHashCode();
        return hash;
    }

    public static bool operator ==(SaveObject? left, SaveObject? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(SaveObject? left, SaveObject? right)
    {
        return !(left == right);
    }
}