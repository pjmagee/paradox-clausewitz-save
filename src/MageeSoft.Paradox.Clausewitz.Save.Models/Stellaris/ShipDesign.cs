namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a ship design in the game state.
/// </summary>
[SaveModel]
public partial class ShipDesign
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get;set; }

    /// <summary>
    /// Gets or sets the ship size.
    /// </summary>
    public string ShipSize { get;set; }
   
}






