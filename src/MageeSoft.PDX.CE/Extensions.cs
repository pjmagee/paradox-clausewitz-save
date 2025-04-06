using System.Text.RegularExpressions;

namespace MageeSoft.PDX.CE;

public static class Extensions
{
    
    
    public static bool TryGetFloats(this SaveObject saveObject, string key, out List<float>? items)
    {
        items = null;
        KeyValuePair<string, SaveElement>? element = saveObject.Properties.FirstOrDefault(p => p.Key == key);

        if (element.HasValue)
        {
            if (element.Value.Value is SaveArray array)
            {
                items = new List<float>();
                
                foreach (var item in array.Items)
                {
                    if (item is Scalar<float> scalar)
                    {
                        items.Add(scalar.Value);
                    }
                    
                    if(item is Scalar<int> intScalar)
                    {
                        items.Add(intScalar.Value);
                    }
                    
                    if(item is Scalar<long> longScalar)
                    {
                        items.Add((float)longScalar.Value);
                    }
                    
                    if (item is Scalar<string> stringScalar)
                    {
                        if (float.TryParse(stringScalar.Value, out float floatValue))
                        {
                            items.Add(floatValue);
                        }
                    }
                }
            }
        }

        return items != null;
    }
    
    public static bool TryGetDoubles(this SaveObject saveObject, string key, out List<double>? items)
    {
        items = null;
        KeyValuePair<string, SaveElement>? element = saveObject.Properties.FirstOrDefault(p => p.Key == key);

        if (element.HasValue)
        {
            if (element.Value.Value is SaveArray array)
            {
                items = new List<double>();
                
                foreach (var item in array.Items)
                {
                    if (item is Scalar<double> scalar)
                    {
                        items.Add(scalar.Value);
                    }
                    
                    if(item is Scalar<int> intScalar)
                    {
                        items.Add(intScalar.Value);
                    }
                    
                    if(item is Scalar<long> longScalar)
                    {
                        items.Add(longScalar.Value);
                    }
                    
                    if (item is Scalar<string> stringScalar)
                    {
                        if (double.TryParse(stringScalar.Value, out double doubleValue))
                        {
                            items.Add(doubleValue);
                        }
                    }
                }
            }
        }

        return items != null;
    }
    
    public static bool TryGetLongs(this SaveObject saveObject, string key, out List<long>? items)
    {
        items = null;
        KeyValuePair<string, SaveElement>? element = saveObject.Properties.FirstOrDefault(p => p.Key == key);

        if (element.HasValue)
        {
            if( element.Value.Value is SaveArray array)
            {
                items = new List<long>();
            
                foreach (var item in array.Items)
                {
                    if (item is Scalar<long> scalar)
                    {
                        items.Add(scalar.Value);
                    }
                    
                    if(item is Scalar<int> intScalar)
                    {
                        items.Add(intScalar.Value);
                    }
                    
                    if(item is Scalar<float> floatScalar)
                    {
                        items.Add((long)floatScalar.Value);
                    }
                    
                    if(item is Scalar<double> doubleScalar)
                    {
                        items.Add((long)doubleScalar.Value);
                    }
                    
                    if(item is Scalar<string> stringScalar)
                    {
                        if (long.TryParse(stringScalar.Value, out long longValue))
                        {
                            items.Add(longValue);
                        }
                    }
                }
            }
        }

        return items != null;
    }
    
    /// <summary>
    ///  Tries to get a list of strings from a SaveObject key=value pair.
    /// </summary>
    /// <param name="obj">
    /// The SaveObject to search for the key in.
    /// </param>
    /// <param name="key">
    /// The key of the property that is an array of strings.
    /// </param>
    /// <param name="items">
    /// The list of strings if the key is found and the value is an array of strings.
    /// </param>
    /// <returns>
    /// true if the key is found and the value is an array of strings; otherwise, false.
    /// </returns>
    public static bool TryGetStrings(this SaveObject obj, string key, out List<string>? items)
    {
        items = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;

        if (element is SaveArray array)
        {
            items = new List<string>();
            
            foreach (var item in array.Items)
            {
                if (item is Scalar<string> scalar)
                {
                    items.Add(scalar.Value);
                }
            }
        }
        
        return items != null;
    }
   
    /// <summary>
    /// Tries to get a list of booleans from a SaveObject key=value pair.
    /// </summary>
    /// <param name="obj">
    /// The SaveObject to search for the key in.
    /// </param>
    /// <param name="key">
    /// The key of the property that is an array of booleans.
    /// </param>
    /// <param name="items">
    /// The list of booleans if the key is found and the value is an array of booleans.
    /// </param>
    /// <returns>
    ///  true if the key is found and the value is an array of booleans; otherwise, false.
    /// </returns>
    public static bool TryGetBools(this SaveObject obj, string key, out List<bool>? items)
    {
        items = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;

        if (element is SaveArray array)
        {
            items = new List<bool>();
            
            foreach (var item in array.Items)
            {
                if (item is Scalar<bool> scalar)
                {
                    items.Add(scalar.Value);
                }
            }
        }
        
        return items != null;
    }
    
    public static T? TryGetScalar<T>(this SaveObject obj, string key)
    {
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<T> scalar)
        {
            return scalar.Value;
        }

        return default(T?);
    }
    
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
    
    public static bool TryGetString(this SaveObject obj, string key, out string? value)
    {
        value = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<string> scalar)
        {
            value = scalar.Value;
            return true;
        }

        return false;
    }

    public static bool TryGetInt(this SaveObject obj, string key, out int? value)
    {
        value = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<int> scalar)
        {
            value = scalar.Value;
            return true;
        }

        return false;
    }

    public static bool TryGetLong(this SaveObject obj, string key, out long? value)
    {
        value = null;
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

    public static bool TryGetFloat(this SaveObject obj, string key, out float? value)
    {
        value = null;
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

    public static bool TryGetBool(this SaveObject obj, string key, out bool? value)
    {
        value = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<bool> boolScalar)
        {
            value = boolScalar.Value;
            return true;
        }

        return false;
    }

    public static bool TryGetGuid(this SaveObject obj, string key, out Guid? value)
    {
        value = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        
        if (element is Scalar<Guid> guidScalar)
        {
            value = guidScalar.Value;
            return true;
        }

        return false;
    }
    
    public static bool TryGetDateTime(this SaveObject obj, string key, out DateTime? value)
    {
        value = null;
        var element = obj.Properties.FirstOrDefault(p => p.Key == key).Value;

        if (element is Scalar<DateTime> dateScalar)
        {
            value = dateScalar.Value;
            return true;
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

    public static bool TryGetSaveArray(this SaveObject obj, string key, out SaveArray value)
    {
        value = null!;
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