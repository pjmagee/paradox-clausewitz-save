using System.Runtime.CompilerServices;

namespace MageeSoft.PDX.CE2;

/// <summary>
/// Extension methods for working with Paradox save elements.
/// </summary>
public static class PdxExtensions
{
    /// <summary>
    /// Creates a new PdxObject from a collection of key-value pairs.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PdxObject ToPdxObject(this IEnumerable<KeyValuePair<PdxString, IPdxElement>> properties) => 
        new(properties);
        
    /// <summary>
    /// Creates a new PdxObject from a collection of string key-value pairs.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PdxObject ToPdxObject(this IEnumerable<KeyValuePair<string, IPdxElement>> properties) => 
        new(properties.Select(p => new KeyValuePair<PdxString, IPdxElement>(new PdxString(p.Key), p.Value)));
        
    /// <summary>
    /// Creates a new PdxArray from a collection of elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PdxArray ToPdxArray(this IEnumerable<IPdxElement> items) => 
        new(items);
        
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