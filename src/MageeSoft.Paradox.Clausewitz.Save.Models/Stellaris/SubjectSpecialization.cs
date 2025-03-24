

namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a subject specialization in the game state.
/// </summary>
[SaveModel]
public partial class SubjectSpecialization
{
    /// <summary>
    /// Gets or sets the level.
    /// </summary>
   
    public float? Level { get;set; }

    /// <summary>
    /// Gets or sets the experience.
    /// </summary>
    public float? Experience { get;set; }

    /// <summary>
    /// Gets or sets the conversion process.
    /// </summary>
    [SaveObject("conversion_process")]
    public ConversionProcess? ConversionProcess { get;set; }
}