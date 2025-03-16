using MageeSoft.Paradox.Clausewitz.SaveReader.Parser;

namespace MageeSoft.Paradox.Clausewitz.SaveReader.Reader.Games.Stellaris.Models;

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
    public ConversionProcess ConversionProcess { get; init; } = ConversionProcess.Default;

    /// <summary>
    /// Default instance of SubjectSpecialization.
    /// </summary>
    public static SubjectSpecialization Default { get; } = new();

    /// <summary>
    /// Loads a subject specialization from a SaveObject.
    /// </summary>
    /// <param name="saveObject">The SaveObject containing the subject specialization data.</param>
    /// <returns>A new SubjectSpecialization instance.</returns>
    public static SubjectSpecialization? Load(SaveObject saveObject)
    {
        if (!saveObject.TryGetFloat("level", out var level) ||
            !saveObject.TryGetFloat("experience", out var experience))
        {
            return null;
        }

        SaveObject? conversionObj;
        var conversion = saveObject.TryGetSaveObject("subject_conversion_process", out conversionObj) && conversionObj != null
            ? ConversionProcess.Load(conversionObj) ?? ConversionProcess.Default
            : ConversionProcess.Default;

        return new SubjectSpecialization
        {
            Level = level,
            Experience = experience,
            ConversionProcess = conversion
        };
    }
}