using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MageeSoft.PDX.CE;

public static class Extensions
{
    public static bool TryGetElement<T>(this IEnumerable<KeyValuePair<string, SaveElement>> properties, string name, out T? value) where T : SaveElement
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
    
    public static bool TryGetDateTime(this SaveObject obj, string key, out DateTime value)
    {
        value = default;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;

        if (element is Scalar<DateTime> dateScalar)
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
                value = new DateTime(year, month, day);
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

    /// <summary>
    /// Serializes a SaveElement to a string in Paradox save file format.
    /// </summary>
    public static string ToSaveString(this SaveElement element)
    {
        var serializer = new Serializer();
        return serializer.Serialize(element);
    }
    
    public static string RemoveFormatting(this string str)
    {
        // remove "extra spaces" and new lines (\r, \n, \r\n)
        str = Regex.Replace(str, @"\r\n|\r|\n", " ");
        str = Regex.Replace(str, @"\s+", " ");
        str = str.Trim();
        return str;
    }

    /// <summary>
    /// Serializes a SaveElement to a file in Paradox save file format.
    /// </summary>
    public static void ToSaveFile(this SaveElement element, string filePath)
    {
        var serializer = new Serializer();
        serializer.SerializeToFile(element, filePath);
    }
} 