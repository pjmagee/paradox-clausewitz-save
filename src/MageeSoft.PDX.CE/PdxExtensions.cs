using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Extension methods for working with Paradox save elements.
/// </summary>
public static class PdxExtensions
{
    /// <summary>
    /// Tries to get a PdxArray value directly.
    /// </summary>
    public static bool TryGetPdxArray(this PdxObject obj, string key, out PdxArray? value)
    {
        value = null;

        if (obj.TryGetValue(key, out var element) && element is PdxArray arr)
        {
            value = arr;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to get a PdxObject value directly.
    /// </summary>
    public static bool TryGetPdxObject(this PdxObject obj, string key, out PdxObject? value)
    {
        value = null;

        if (obj.TryGetValue(key, out var element) && element is PdxObject objVal)
        {
            value = objVal;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to get a string value directly with nullable output.
    /// </summary>
    public static bool TryGetString(this PdxObject obj, string key, out string? value)
    {
        return obj.TryGetString(key, out value);
    }

    /// <summary>
    /// Tries to get an int value directly with nullable output.
    /// </summary>
    public static bool TryGetInt(this PdxObject obj, string key, out int? value)
    {
        value = null;
        if (obj.TryGetInt(key, out int intValue))
        {
            value = intValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tries to get a long value directly with nullable output.
    /// </summary>
    public static bool TryGetLong(this PdxObject obj, string key, out long? value)
    {
        value = null;
        if (obj.TryGetLong(key, out long longValue))
        {
            value = longValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tries to get a float value directly with nullable output.
    /// </summary>
    public static bool TryGetFloat(this PdxObject obj, string key, out float? value)
    {
        value = null;
        if (obj.TryGetFloat(key, out float floatValue))
        {
            value = floatValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tries to get a bool value directly with nullable output.
    /// </summary>
    public static bool TryGetBool(this PdxObject obj, string key, out bool? value)
    {
        value = null;
        if (obj.TryGetBool(key, out bool boolValue))
        {
            value = boolValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tries to get a DateTime value directly with nullable output.
    /// </summary>
    public static bool TryGetDateTime(this PdxObject obj, string key, out DateTime? value)
    {
        value = null;
        if (obj.TryGetDate(key, out DateTime dateValue))
        {
            value = dateValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tries to get a Guid value directly with nullable output.
    /// </summary>
    public static bool TryGetGuid(this PdxObject obj, string key, out Guid? value)
    {
        value = null;
        if (obj.TryGetGuid(key, out Guid guidValue))
        {
            value = guidValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a new PdxObject from a collection of key-value pairs.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PdxObject ToPdxObject(this IEnumerable<KeyValuePair<PdxString, IPdxElement>> properties)
    {
        return new PdxObject([..properties]);
    }

    /// <summary>
    /// Creates a new PdxObject from a collection of string key-value pairs.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PdxObject ToPdxObject(this IEnumerable<KeyValuePair<string, IPdxElement>> properties)
    {
        var immutableProperties = properties
            .Select(p => new KeyValuePair<PdxString, IPdxElement>(new PdxString(p.Key), p.Value))
            .ToImmutableArray();
        
        return new PdxObject(immutableProperties);
    }

    /// <summary>
    /// Creates a new PdxObject from a List of string key-value pairs.
    /// This overload is specifically for the generated code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PdxObject ToPdxObject(this List<KeyValuePair<string, IPdxElement>> properties)
    {
        return properties.ToPdxObject();
    }

    /// <summary>
    /// Creates a new PdxArray from a collection of elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PdxArray ToPdxArray(this IEnumerable<IPdxElement> items) => new(items.ToImmutableArray());

    /// <summary>
    /// Creates a new PdxArray from a List of elements.
    /// This overload is specifically for the generated code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PdxArray ToPdxArray(this List<IPdxElement> items) => items.ToPdxArray();

    /// <summary>
    /// Convenience method for reading a save file from a file path.
    /// </summary>
    public static PdxObject ReadFile(string filePath)
    {
        return PdxSaveReader.Read(File.ReadAllText(filePath));
    }

    /// <summary>
    /// Converts a primitive value to a PDX scalar.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IPdxElement ToPdxScalar<T>(this T value) where T : notnull
    {
        return value switch
        {
            string s => new PdxString(s),
            bool b => new PdxBool(b),
            int i => new PdxInt(i),
            long l => new PdxLong(l),
            float f => new PdxFloat(f),
            DateTime d => new PdxDate(d),
            Guid g => new PdxGuid(g),
            _ => new PdxString(value.ToString() ?? string.Empty)
        };
    }
}