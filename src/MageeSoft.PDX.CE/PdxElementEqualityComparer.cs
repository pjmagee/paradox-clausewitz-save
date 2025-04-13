using System.Runtime.CompilerServices;

namespace MageeSoft.PDX.CE;

/// <summary>
/// Static helper class for comparing PDX elements.
/// </summary>
public static class PdxElementEqualityComparer
{
    /// <summary>
    /// Determines whether two IPdxElement instances are equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(IPdxElement? a, IPdxElement? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        if (a.Type != b.Type) return false;
        
        return a switch
        {
            PdxObject objA => b is PdxObject objB && objA.Equals(objB),
            PdxArray arrA => b is PdxArray arrB && arrA.Equals(arrB),
            PdxString strA => b is PdxString strB && strA.Equals(strB),
            PdxBool boolA => b is PdxBool boolB && boolA.Equals(boolB),
            PdxInt intA => b is PdxInt intB && intA.Equals(intB),
            PdxLong longA => b is PdxLong longB && longA.Equals(longB),
            PdxFloat floatA => b is PdxFloat floatB && floatA.Equals(floatB),
            PdxDate dateA => b is PdxDate dateB && dateA.Equals(dateB),
            PdxGuid guidA => b is PdxGuid guidB && guidA.Equals(guidB),
            _ => a.Equals(b)
        };
    }
} 