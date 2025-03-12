using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

public static class SaveObjectHelper
{
    public static string GetValue(SaveObject obj, string key, string defaultValue)
    {
        var element = obj.Properties.FirstOrDefault(p => p.Key == key);

        if (element.Key != null)
        {
            if (element.Value is Scalar<string> scalar)
            {
                return scalar.RawText;
            }
            return element.Value.ToString().Replace("Scalar: ", "");
        }

        return defaultValue;
    }

    public static bool GetBoolValue(SaveObject obj, string key, bool defaultValue)
    {
        var element = obj.Properties.FirstOrDefault(p => p.Key == key);

        if (element.Key != null)
        {
            if (element.Value is Scalar<string> scalar)
            {
                return scalar.Value.ToLowerInvariant() == "yes";
            }
            else if (element.Value is Scalar<bool> boolScalar)
            {
                return boolScalar.Value;
            }
            var str = element.Value.ToString().Replace("Scalar: ", "").ToLowerInvariant();
            return str == "yes" || str == "true";
        }

        return defaultValue;
    }

    public static int GetIntValue(SaveObject obj, string key, int defaultValue)
    {
        var element = obj.Properties.FirstOrDefault(p => p.Key == key);

        if (element.Key != null)
        {
            if (element.Value is Scalar<int> scalar)
            {
                return scalar.Value;
            }
            var str = element.Value.ToString().Replace("Scalar: ", "");
            if (int.TryParse(str, out var parsed))
            {
                return parsed;
            }
        }

        return defaultValue;
    }

    public static long GetLongValue(SaveObject obj, string key, long defaultValue)
    {
        var element = obj.Properties.FirstOrDefault(p => p.Key == key);

        if (element.Key != null)
        {
            if (element.Value is Scalar<long> scalar)
            {
                return scalar.Value;
            }
            var str = element.Value.ToString().Replace("Scalar: ", "");
            if (long.TryParse(str, out var parsed))
            {
                return parsed;
            }
        }

        return defaultValue;
    }

    public static float GetFloatValue(SaveObject obj, string key, float defaultValue)
    {
        var element = obj.Properties.FirstOrDefault(p => p.Key == key);

        if (element.Key != null)
        {
            if (element.Value is Scalar<float> scalar)
            {
                return scalar.Value;
            }
            var str = element.Value.ToString().Replace("Scalar: ", "");
            if (float.TryParse(str, out var parsed))
            {
                return parsed;
            }
        }

        return defaultValue;
    }

    public static string? GetScalarString(SaveObject obj, string key)
    {
        var scalar = obj.Properties.FirstOrDefault(p => p.Key == key).Value as Scalar<string>;
        return scalar?.Value;
    }

    public static int GetScalarInt(SaveObject obj, string key)
    {
        var scalar = obj.Properties.FirstOrDefault(p => p.Key == key).Value as Scalar<int>;
        return scalar?.Value ?? 0;
    }

    public static long GetScalarLong(SaveObject obj, string key)
    {
        var scalar = obj.Properties.FirstOrDefault(p => p.Key == key).Value as Scalar<long>;
        return scalar?.Value ?? 0;
    }

    public static float GetScalarFloat(SaveObject obj, string key)
    {
        var scalar = obj.Properties.FirstOrDefault(p => p.Key == key).Value as Scalar<float>;
        return scalar?.Value ?? 0;
    }

    public static bool GetScalarBoolean(SaveObject obj, string key)
    {
        var value = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        if (value is Scalar<bool> boolScalar)
            return boolScalar.Value;
        return false;
    }

    public static DateOnly GetScalarDateOnly(SaveObject obj, string key)
    {
        var value = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        if (value is Scalar<DateOnly> dateScalar)
            return dateScalar.Value;
        return default;
    }

    public static SaveObject? GetObject(SaveObject obj, string key)
    {
        var value = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        return value as SaveObject;
    }

    public static SaveArray? GetArray(SaveObject obj, string key)
    {
        var value = obj.Properties.FirstOrDefault(p => p.Key == key).Value;
        return value as SaveArray;
    }
} 