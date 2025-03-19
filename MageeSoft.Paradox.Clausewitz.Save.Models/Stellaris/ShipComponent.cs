namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a ship component in the game state.
/// </summary>
[SaveModel]
public partial class ShipComponent
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get;set; }

    /// <summary>
    /// Gets or sets the template.
    /// </summary>
    public string Template { get;set; }
}







