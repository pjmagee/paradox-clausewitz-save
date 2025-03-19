namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a movement target in the game state.
/// </summary>
[SaveModel]
public partial class MovementTarget
{
    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    [SaveObject("coordinate")]
    public Coordinate Coordinate { get;set; }

}






