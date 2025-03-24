namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a movement formation in the game state.
/// </summary>  
[SaveModel]
public partial class MovementFormation
{
    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    public float? Scale { get;set; }

    /// <summary>
    /// Gets or sets the angle.
    /// </summary>
    public float? Angle { get;set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string? Type { get;set; }
}






