namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a megastructure in the game state.
/// </summary>  
[SaveModel]
public partial class Megastructure
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    [SaveScalar("type")]
    public string? Type { get;set; }

    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    [SaveObject("coordinate")]
    public Coordinate? Coordinate { get;set; }
    
    [SaveScalar("owner")]
    public int? Owner { get; set; }
}