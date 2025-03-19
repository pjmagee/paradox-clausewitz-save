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
    public required Coordinate Coordinate { get;set; }

    /// <summary>
    /// Gets or sets the attach information.
    /// </summary>
    public required AttachInfo Attach { get;set; }

    /// <summary>
    /// Gets or sets the offset values.
    /// </summary>
    public required ImmutableArray<float> Offset { get;set; }

    /// <summary>
    /// Gets or sets the scale value.
    /// </summary>
    public required int Scale { get;set; }

    /// <summary>
    /// Gets or sets the entity face object information.
    /// </summary>
    public required AttachInfo EntityFaceObject { get;set; }

    /// <summary>
    /// Gets or sets the appear state.
    /// </summary>
    public required string AppearState { get;set; }

}






