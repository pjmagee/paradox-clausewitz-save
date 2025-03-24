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
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long? Id { get;set; }

    /// <summary>
    /// Gets or sets the opener ID.
    /// </summary>
    public long? OpenerId { get;set; }

    /// <summary>
    /// Gets or sets the random values.
    /// </summary>
    public List<long>? Random { get;set; }

    /// <summary>
    /// Gets or sets whether random is allowed.
    /// </summary>
    public bool? RandomAllowed { get;set; }
}