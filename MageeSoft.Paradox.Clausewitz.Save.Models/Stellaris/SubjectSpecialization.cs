using MageeSoft.Paradox.Clausewitz.Save.Models.Attributes;

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a subject specialization in the game state.
/// </summary>
public class SubjectSpecialization
{
    /// <summary>
    /// Gets or sets the level.
    /// </summary>
   
    public float Level { get; init; }

    /// <summary>
    /// Gets or sets the experience.
    /// </summary>
    public float Experience { get; init; }

    /// <summary>
    /// Gets or sets the conversion process.
    /// </summary>
    [SaveObject("conversion_process")]
    public ConversionProcess ConversionProcess { get; init; }
}