namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

/// <summary>
/// Represents attach information for an ambient object.
/// </summary>
public class AttachInfo
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required int Type { get; init; }

    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }
}