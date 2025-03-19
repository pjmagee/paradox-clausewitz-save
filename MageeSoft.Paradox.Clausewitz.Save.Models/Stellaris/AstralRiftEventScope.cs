using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents the scope of an astral rift event.
/// </summary>
[SaveModel]
public partial class AstralRiftEventScope
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the opener ID.
    /// </summary>
    public required long OpenerId { get;set; }

    /// <summary>
    /// Gets or sets the random values.
    /// </summary>
    public required ImmutableArray<long> Random { get;set; }

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public required bool RandomAllowed { get;set; }

    /// <summary>
    /// Default instance of AstralRiftEventScope.
    /// </summary>
    public static AstralRiftEventScope Default => new()
    {
        Type = string.Empty,
        Id = 0,
        OpenerId = 0,
        Random = ImmutableArray<long>.Empty,
        RandomAllowed = false
    };
}