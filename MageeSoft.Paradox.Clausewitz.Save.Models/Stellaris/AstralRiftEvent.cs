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
    public AstralRiftEventScope Scope { get;set; }

    /// <summary>
    /// Gets or sets the effect.
    /// </summary>
    public string Effect { get;set; }

    /// <summary>
    /// Gets or sets the picture.
    /// </summary>
    public string Picture { get;set; }

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    public int Index { get;set; }

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