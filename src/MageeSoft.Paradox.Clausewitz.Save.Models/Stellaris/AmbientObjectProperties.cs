using System.Collections.Immutable;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents the properties of an ambient object.
/// </summary>
[SaveModel]
public partial class AmbientObjectProperties
{
    /// <summary>
    /// Gets or sets the coordinate of the properties.
    /// </summary>
    public Coordinate? Coordinate { get;set; }

    /// <summary>
    /// Gets or sets the attach information.
    /// </summary>
    public AttachInfo? Attach { get;set; }

    /// <summary>
    /// Gets or sets the offset values.
    /// </summary>
    public List<float>? Offset { get;set; }

    /// <summary>
    /// Gets or sets the scale value.
    /// </summary>
    public int? Scale { get;set; }

    /// <summary>
    /// Gets or sets the entity face object information.
    /// </summary>
    public AttachInfo? EntityFaceObject { get;set; }

    /// <summary>
    /// Gets or sets the appear state.
    /// </summary>
    public string? AppearState { get;set; }

}






