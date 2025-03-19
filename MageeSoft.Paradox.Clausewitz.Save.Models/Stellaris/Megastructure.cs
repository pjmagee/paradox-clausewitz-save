namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a megastructure in the game state.
/// </summary>  
[SaveModel]
public partial class Megastructure
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets the stage.
    /// </summary>
    public string Stage { get;set; }

    /// <summary>
    /// Gets or sets the coordinate.
    /// </summary>
    public Coordinate Coordinate { get;set; }

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public float Progress { get;set; }

    /// <summary>
    /// Gets or sets whether the megastructure is active.
    /// </summary>
    public bool IsActive { get;set; }
}