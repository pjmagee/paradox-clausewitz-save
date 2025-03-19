namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an ambient object in the game state.
/// </summary>  
[SaveModel]
public partial class AmbientObject
{
    /// <summary>
    /// Gets or sets the ambient object ID.
    /// </summary>
    public long Id { get;set; }

    /// <summary>
    /// Gets or sets the type of the ambient object.
    /// </summary>
    public string Type { get;set; }

    /// <summary>
    /// Gets or sets a value indicating whether the ambient object is active.
    /// </summary>
    public bool IsActive { get;set; }

    /// <summary>
    /// Gets or sets the coordinate of the ambient object.
    /// </summary>
    public Coordinate Coordinate { get;set; }

    /// <summary>
    /// Gets or sets the data type of the ambient object.
    /// </summary>
    public string Data { get;set; }

    /// <summary>
    /// Gets or sets the properties of the ambient object.
    /// </summary>
    public AmbientObjectProperties Properties { get;set; }
}