using System.Collections.Immutable;
using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

public static class Extensions
{
    public static bool TryGetElement<T>(this ImmutableArray<KeyValuePair<string, SaveElement>> properties, string name, out T? value) where T : SaveElement
    {
        foreach (var property in properties)
        {
            if (property.Key == name)
            {
                if (property.Value is T element)
                {
                    value = element;
                    return true;
                }
            }
        }
        
        value = null!;
        return false;
    }

    /// <summary>
    /// Gets the elements of a SaveArray.
    /// </summary>
    /// <param name="array">The SaveArray.</param>
    /// <returns>An immutable array of SaveElements.</returns>
    public static ImmutableArray<SaveElement> Elements(this SaveArray array)
    {
        return array.Items;
    }
    
    public static bool TryGetString(this SaveObject obj, string key, out string value)
    {
        value = string.Empty;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<string> scalar)
        {
            value = scalar.Value;
            return true;
        }

        return false;
    }

    public static bool TryGetInt(this SaveObject obj, string key, out int value)
    {
        value = 0;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<int> scalar)
        {
            value = scalar.Value;
            return true;
        }

        return false;
    }

    public static bool TryGetLong(this SaveObject obj, string key, out long value)
    {
        value = 0;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<long> longScalar)
        {
            value = longScalar.Value;
            return true;
        }
        
        if (element is Scalar<int> intScalar)
        {
            value = intScalar.Value;
            return true;
        }

        return false;
    }

    public static bool TryGetFloat(this SaveObject obj, string key, out float value)
    {
        value = 0;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<float> scalar)
        {
            value = scalar.Value;
            return true;
        }

        if (element is Scalar<int> intScalar)
        {
            value = intScalar.Value;
            return true;
        }

        return false;
    }

    public static bool TryGetBool(this SaveObject obj, string key, out bool value)
    {
        value = false;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<bool> boolScalar)
        {
            value = boolScalar.Value;
            return true;
        }

        return false;
    }

    public static bool TryGetGuid(this SaveObject obj, string key, out Guid value)
    {
        value = Guid.Empty;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<Guid> guidScalar)
        {
            value = guidScalar.Value;
            return true;
        }

        return false;
    }
    
    public static bool TryGetDateOnly(this SaveObject obj, string key, out DateOnly value)
    {
        value = default;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;

        if (element is Scalar<DateOnly> dateScalar)
        {
            value = dateScalar.Value;
            return true;
        }

        if (element is Scalar<string> stringScalar)
        {
            var parts = stringScalar.Value.Split('.');
            if (parts.Length == 3 && 
                int.TryParse(parts[0], out var year) && 
                int.TryParse(parts[1], out var month) && 
                int.TryParse(parts[2], out var day))
            {
                value = new DateOnly(year, month, day);
                return true;
            }
        }

        return false;
    }

    public static bool TryGetSaveObject(this SaveObject obj, string key, out SaveObject? value)
    {
        value = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is SaveObject saveObj)
        {
            value = saveObj;
            return true;
        }

        return false;
    }

    public static bool TryGetSaveArray(this SaveObject obj, string key, out SaveArray? value)
    {
        value = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is SaveArray array)
        {
            value = array;
            return true;
        }

        return false;
    }
} 