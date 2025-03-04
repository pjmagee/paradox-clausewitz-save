using System;
using System.Collections.Generic;

public static class Extensions
{
    public static IEnumerable<KeyValuePair<string, Element>> EnumerateObject(this Element element)
    {
        if (element is Object obj)
            return obj.Properties;
        throw new InvalidOperationException("Element is not an object.");
    }

    public static IEnumerable<Element> EnumerateArray(this Element element)
    {
        if (element is Array arr)
            return arr.Items;
        throw new InvalidOperationException("Element is not an array.");
    }

    public static IEnumerable<Element> EnumerateElements(this Element element)
    {
        if (element is Object obj)
        {
            foreach (var prop in obj.Properties)
                yield return prop.Value;
        }
        else if (element is Array arr)
        {
            foreach (var item in arr.Items)
                yield return item;
        }
        else
        {
            throw new InvalidOperationException("Element is neither an object nor an array.");
        }
    }
}