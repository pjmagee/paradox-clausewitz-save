namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents debris in the game state.
/// </summary>  
[SaveModel]
public partial class Debris
{
    /// <summary>
    /// Gets or sets the debris ID.
    /// </summary>
    public long? Id { get;set; }

    /// <summary>
    /// Gets or sets the country ID that owns the debris.
    /// </summary>
    public long? Owner { get;set; }

    /// <summary>
    /// Gets or sets the coordinate of the debris.
    /// </summary>
    public Position? Position { get;set; }

    /// <summary>
    /// Gets or sets the type of the debris.
    /// </summary>
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets whether the debris is active.
    /// </summary>
    public bool? IsActive { get;set; }

    /// <summary>
    /// Gets or sets whether the debris is visible.
    /// </summary>
    public bool? IsVisible { get;set; }

    /// <summary>
    /// Gets or sets whether the debris is hostile.
    /// </summary>
    public bool? IsHostile { get;set; }

} 






