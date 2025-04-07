namespace MageeSoft.PDX.CE2;

/// <summary>
/// Helper class for comparing SaveElement instances.
/// </summary>
public static class PdxElementComparer
{
    /// <summary>
    /// Determines whether two SaveElement instances are equal.
    /// </summary>
    /// <param name="a">The first SaveElement to compare.</param>
    /// <param name="b">The second SaveElement to compare.</param>
    /// <returns>true if the instances are equal; otherwise, false.</returns>
    public static bool Equals(PdxElement? a, PdxElement? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        if (a.Type != b.Type) return false;

        return a.Type switch
        {
            PdxType.Object => ((PdxObject)a).Equals((PdxObject)b),
            PdxType.Array => ((PdxArray)a).Equals((PdxArray)b),
            PdxType.String => ((PdxScalar<string>)a).Equals((PdxScalar<string>)b),
            PdxType.Identifier => ((PdxScalar<string>)a).Equals((PdxScalar<string>)b),
            PdxType.Bool => ((PdxScalar<bool>)a).Equals((PdxScalar<bool>)b),
            PdxType.Float => ((PdxScalar<float>)a).Equals((PdxScalar<float>)b),
            PdxType.Int32 => ((PdxScalar<int>)a).Equals((PdxScalar<int>)b),
            PdxType.Int64 => ((PdxScalar<long>)a).Equals((PdxScalar<long>)b),
            PdxType.Date => ((PdxScalar<DateTime>)a).Equals((PdxScalar<DateTime>)b),
            PdxType.Guid => ((PdxScalar<Guid>)a).Equals((PdxScalar<Guid>)b),
            _ => false
        };
    }
} 