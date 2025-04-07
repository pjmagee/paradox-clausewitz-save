namespace MageeSoft.PDX.CE2;

/// <summary>
/// Extension methods for working with SaveElement objects.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Try to get a value from a SaveObject by key.
    /// </summary>
    /// <param name="pdxObject">The SaveObject to search in.</param>
    /// <param name="key">The key to look for.</param>
    /// <param name="value">The found value, if any.</param>
    /// <returns>True if the key was found and the value could be retrieved, otherwise false.</returns>
    public static bool TryGetValue(this PdxObject pdxObject, string key, out PdxElement? value)
    {
        value = null;
        foreach (var property in pdxObject.Properties)
        {
            if (property.Key == key)
            {
                value = property.Value;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Try to get a scalar value from a SaveObject by key.
    /// </summary>
    /// <typeparam name="T">The type of the scalar value.</typeparam>
    /// <param name="pdxObject">The SaveObject to search in.</param>
    /// <param name="key">The key to look for.</param>
    /// <param name="value">The found scalar value, if any.</param>
    /// <returns>True if the key was found and the value could be retrieved as the specified type, otherwise false.</returns>
    public static bool TryGetScalar<T>(this PdxObject pdxObject, string key, out T? value) where T : notnull
    {
        value = default;
        if (pdxObject.TryGetValue(key, out var element) && element is PdxScalar<T> scalar)
        {
            value = scalar.Value;
            return true;
        }
        return false;
    }
} 