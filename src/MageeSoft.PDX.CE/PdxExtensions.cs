using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MageeSoft.PDX.CE;

public static class PdxExtensions
{
    /// <summary>
    /// Tries to get a property value by key.
    /// </summary>
    public static bool TryGetValue(this PdxObject pdxObject, string key, [NotNullWhen(true)] out IPdxElement? value)
    {
        foreach (var prop in pdxObject.Properties)
        {
            if (prop.Key is PdxString pdxString && pdxString.Value.Equals(key, StringComparison.CurrentCulture))
            {
                value = prop.Value;
                return true;
            }
            
            if (prop.Key is PdxInt pdxInt && int.TryParse(key, out int intKey) && pdxInt.Value == intKey)
            {
                value = prop.Value;
                return true;
            }
            
            if (prop.Key is PdxLong pdxLong && long.TryParse(key, out long longKey) && pdxLong.Value == longKey)
            {
                value = prop.Value;
                return true;
            }
            
            if (prop.Key is PdxFloat pdxFloat && float.TryParse(key, out float floatKey) && pdxFloat.Value == floatKey)
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
    public static bool TryGet<T>(this PdxObject pdxObject, string key, [NotNullWhen(true)] out T? value) where T : class, IPdxElement
    {
        if (pdxObject.TryGetValue(key, out var element) && element is T typedValue)
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
    public static bool TryGetString(this PdxObject pdxObject, string key, out string? value)
    {
        if (pdxObject.TryGetValue(key, out var element) && element is PdxString str)
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
    public static bool TryGetBool(this PdxObject pdxObject, string key, out bool value)
    {
        if (pdxObject.TryGetValue(key, out var element) && element is PdxBool b)
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
    public static bool TryGetInt(this PdxObject pdxObject, string key, out int value)
    {
        if (pdxObject.TryGetValue(key, out var element) && element is PdxInt i)
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
    public static bool TryGetLong(this PdxObject pdxObject, string key, out long value)
    {
        if (pdxObject.TryGetValue(key, out var element) && element is PdxLong l)
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
    public static bool TryGetFloat(this PdxObject pdxObject, string key, out float value)
    {
        if (pdxObject.TryGetValue(key, out var element) && element is PdxFloat f)
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
    public static bool TryGetDate(this PdxObject pdxObject, string key, out DateOnly value)
    {
        if (pdxObject.TryGetValue(key, out var element) && element is PdxDate d)
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
    public static bool TryGetGuid(this PdxObject pdxObject, string key, out Guid value)
    {
        if (pdxObject.TryGetValue(key, out var element) && element is PdxGuid g)
        {
            value = g.Value;
            return true;
        }

        value = default;
        return false;
    }

     /// <summary>
    /// This is keys in a csf file can be something like my_key_name.
    /// We want to convert this to MyKeyName.
    /// </summary>    
    public static string ToTitleCase(this IPdxScalar value)
    {
        if (value is PdxString pdxString)
        {
            return string.Join(string.Empty, pdxString.Value
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(x)));
        }

        return value.Value<string>();
    }

    public static bool IsObjectArray(this PdxArray  array)
    {
        return array.Items.All(i => i is PdxObject);
    }

    public static bool IsStringArray(this PdxArray  array)
    {
        return array.Items.All(i => i is PdxString);
    }

    public static bool IsNumberArray(this PdxArray array) 
    {
        return array.Items.All(i => i is PdxInt or PdxLong);
    }

    public static bool IsBoolArray(this PdxArray array)
    {
        return array.Items.All(i => i is PdxBool);
    }

    public static bool IsDateArray(this PdxArray array)
    {
        return array.Items.All(i => i is PdxDate);
    }

    public static bool IsPdxDictionary<TPdxScalar, TPdxElement>(this IPdxElement pdxElement) where TPdxScalar : IPdxScalar where TPdxElement : IPdxElement
    {
        if (pdxElement is PdxObject pdxObject)
        {
            return pdxObject.Properties.All(x => (x.Key is TPdxScalar) && x.Value is TPdxElement);
        }

        return false;
    }
    
    public static bool IsArrayOfIdObjectPairArrays(this PdxArray pdxArray)
    {
        // each inner array must be a pair of int and object    
        return pdxArray.Items.All(i => i is PdxArray array && array.Items.Count == 2 && array.Items[0] is PdxInt or PdxLong && array.Items[1] is PdxObject);
    }

    public static bool ContainsDuplicateKeys(this PdxObject pdxObject)
    {
        return pdxObject.Properties.GroupBy(x => x.Key).Any(g => g.Count() > 1);
    }

    public static IEnumerable<IPdxScalar> GetKeysWithMultipleValues(this PdxObject pdxObject)
    {
        return pdxObject.Properties.GroupBy(x => x.Key).Where(g => g.Count() > 1).Select(g => g.Key);
    }
    
    public static bool IsPdxObjectCollectionNoneable(this ImmutableArray<KeyValuePair<IPdxScalar, IPdxElement>> array)
    {
        // When an array of key=value are objects, with an exception of the ability to be "none"
        // This makes the collection a 'nullable' e.g   Dictionary<int, T?> Items
        return array.Any(x => x.Value is PdxObject) && array.Where(i => i.Value is PdxString).All(x => x.Value is PdxString s && s.Value == "none");
    }

    public static IEnumerable<IPdxScalar> GetNoneableKeys(this PdxObject pdxObject)
    {
        return pdxObject.Properties.Where(x => x.Value is PdxString s && s.Value == "none").Select(x => x.Key);
    }

    public static bool IsPdxDictionary<TPdxScalar, TPdxElement>(this PdxObject pdxObject) where TPdxScalar : IPdxScalar where TPdxElement : IPdxElement
    {
        return pdxObject.Properties.All(x => (x.Key is TPdxScalar) && x.Value is TPdxElement);
    }

    public static bool IsPdxDictionary(this PdxObject pdxObject)
    {
        return pdxObject.Properties.All(x => x.Key is PdxInt or PdxLong);
    }

    public static bool IsPdxIntDictionary(this PdxObject pdxObject)
    {
        return pdxObject.Properties.All(x => x.Key is PdxInt);
    }

    public static bool IsPdxLongDictionary(this PdxObject pdxObject)
    {
        return pdxObject.Properties.Any(x => x.Key is PdxLong);
    }

    
    public static T Value<T>(this IPdxElement scalar)
    {
        if (scalar is PdxString pdxString)
        {
            return (T)Convert.ChangeType(pdxString.Value, typeof(T));
        }

        if (scalar is PdxInt pdxInt)
        {
            return (T)Convert.ChangeType(pdxInt.Value, typeof(T));
        }

        if (scalar is PdxLong pdxLong)
        {
            return (T)Convert.ChangeType(pdxLong.Value, typeof(T));
        }

        if (scalar is PdxFloat pdxFloat)
        {
            return (T)Convert.ChangeType(pdxFloat.Value, typeof(T));
        }

        if (scalar is PdxBool pdxBool)
        {
            return (T)Convert.ChangeType(pdxBool.Value, typeof(T)); 
        }

        if (scalar is PdxDate pdxDate)
        {
            return (T)Convert.ChangeType(pdxDate.Value, typeof(T));
        }

        if (scalar is PdxGuid pdxGuid)
        {
            return (T)Convert.ChangeType(pdxGuid.Value, typeof(T));
        }

        throw new InvalidOperationException($"Unknown scalar type: {scalar.GetType()}");
    }
    
    /// <summary>
    /// Helper method to find a property by key string in a PdxObject's properties
    /// </summary>
    public static KeyValuePair<IPdxScalar, IPdxElement> FindProperty(this PdxObject obj, string key)
    {
        foreach (var property in obj.Properties)
        {
            if (property.Key is PdxString pdxString && pdxString.Value == key)
            {
                return property;
            }
            else if (property.Key is PdxInt pdxInt && pdxInt.Value.ToString() == key)
            {
                return property;
            }
            else if (property.Key is PdxLong pdxLong && pdxLong.Value.ToString() == key)
            {
                return property;
            }
        }
        
        throw new InvalidOperationException($"Property with key '{key}' not found");
    }
} 