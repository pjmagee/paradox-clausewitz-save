namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an astral rift event.
/// </summary>  
[SaveModel]
public partial class AstralRiftEvent
{
    /// <summary>
    /// Gets or sets the scope information.
    /// </summary>
    public required AstralRiftEventScope Scope { get;set; }

    /// <summary>
    /// Gets or sets the effect.
    /// </summary>
    public required string Effect { get;set; }

    /// <summary>
    /// Gets or sets the picture.
    /// </summary>
    public required string Picture { get;set; }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public required int Index { get;set; }

    /// <summary>
    /// Default instance of AstralRiftEvent.
    /// </summary>
    public static AstralRiftEvent Default => new()
    {
        Scope = AstralRiftEventScope.Default,
        Effect = string.Empty,
        Picture = string.Empty,
        Index = 0
    };
}